using System;
using System.Collections.Generic;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Cryptography;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Queues;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.Plugins.Display.Panasonic.Projector
{
    /// <summary>
    /// Plugin device template for third party devices that use IBasicCommunication
    /// </summary>
    public class PanasonicProjectorController : TwoWayDisplayBase, IBridgeAdvanced, ICommunicationMonitor
#if SERIES4
        , IHasInputs<byte, int>
#endif
    {
        private const long DefaultWarmUpTimeMs = 1000;
        private const long DefaultCooldownTimeMs = 2000;
        
        private readonly PanasonicProjectorConfig _config;
        private readonly ICommandBuilder _commandBuilder;
        private readonly IBasicCommunication _comms;
        private readonly CommunicationGather _commsGather;
        private readonly StatusMonitorBase _commsMonitor;
        private readonly MD5CryptoServiceProvider _md5Provider;
        private readonly GenericQueue _rxQueue;
        private readonly CrestronQueue<string> _txQueue;

        private bool _powerIsOn;
        private bool _powerOnIgnoreFb;
        private bool _isWarming;
        private bool _isCooling;
        private string _currentCommand;
        private eInputTypes _currentInput; 
        private string _hash;
        
        public StatusMonitorBase CommunicationMonitor
        {
            get { return _commsMonitor; }
        }

        /// <summary>
        /// Reports online feedback through the bridge
        /// </summary>
        public BoolFeedback OnlineFeedback
        {
            get { return _commsMonitor.IsOnlineFeedback; }
        }

        /// <summary>
        /// Reports socket status feedback through the bridge
        /// </summary>
        public IntFeedback StatusFeedback { get; private set; }

        /// <summary>
        /// Reports connect feedback through the bridge
        /// </summary>
        public BoolFeedback ConnectFeedback { get; private set; }


        /// <summary>
        /// Plugin device constructor for devices that need IBasicCommunication
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="config"></param>
        /// <param name="comms"></param>
        public PanasonicProjectorController(string key, string name, PanasonicProjectorConfig config,
            IBasicCommunication comms)
            : base(key, name)
        {
            Debug.Console(0, this, "Constructing new {0} instance", name);

            _rxQueue = new GenericQueue(String.Format("{0}-rxQueue", Key));
            _txQueue = new CrestronQueue<string>(50);

            _config = config;
            WarmupTime = _config.WarmupTimeInSeconds == 0 ? (uint)DefaultWarmUpTimeMs : (uint)_config.WarmupTimeInSeconds * 1000;
            CooldownTime = _config.CooldownTimeInSeconds == 0 ? (uint)DefaultCooldownTimeMs : (uint)_config.CooldownTimeInSeconds * 1000;

            ConnectFeedback = new BoolFeedback(() => Connect);
            StatusFeedback = new IntFeedback(() => (int)_commsMonitor.Status);

            _comms = comms;
            _commandBuilder = GetCommandBuilder(_config);

            if (_commandBuilder == null)
            {
                Debug.Console(0, Debug.ErrorLogLevel.Error,
                    "Command builder not created. Unable to continue. Please correct the configuration to use either 'com' or 'tcpip' as the control method");
                return;
            }

            _commsMonitor = new PanasonicStatusMonitor(this, _comms, 60000, 120000);

            var socket = _comms as ISocketStatus;
            if (socket != null)
            {
                // device comms is IP **ELSE** device comms is RS232
                socket.ConnectionChange += socket_ConnectionChange;

                _md5Provider = new MD5CryptoServiceProvider();
                _md5Provider.Initialize();
            }

            var commsDelimiter = _commandBuilder.Delimiter;
            // _comms gather for any API that has a defined delimiter
            _commsGather = new CommunicationGather(_comms, commsDelimiter);
            _commsGather.LineReceived += Handle_LineRecieved;

            SetupInputPorts();
        }

        /// <summary>
        /// Initializes device
        /// </summary>
        public override void Initialize()
        {
            _commsMonitor.Start();

            var pollTimer = new CTimer(_ =>
            {
                try
                {
                    SendText(_commandBuilder.GetCommand("QPW"));
                }
                catch (Exception ex)
                {
                    Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Caught an exception in the poll:{0}", ex.Message);
                    throw;
                }
            }, null, 50000, 50000);

            CrestronEnvironment.ProgramStatusEventHandler += type =>
            {
                if (type != eProgramStatusEventType.Stopping)
                    return;

                pollTimer.Stop();
                pollTimer.Dispose();
                _commsMonitor.Stop();
            };
        }        

        #region Implementation of IBridgeAdvanced

        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            LinkDisplayToApi(this, trilist, joinStart, joinMapKey, bridge);
        }

        #endregion
        
        private ICommandBuilder GetCommandBuilder(PanasonicProjectorConfig config)
        {
            if (config.Control.Method == eControlMethod.Com)
            {
                return new SerialCommandBuilder(config.Id);
            }

            if (config.Control.Method == eControlMethod.Tcpip)
            {
                return new IpCommandBuilder();
            }

            Debug.Console(0, this, Debug.ErrorLogLevel.Error, "Control method {0} isn't valid for this plugin.",
                config.Control.Method);
            return null;
        }


        private void socket_ConnectionChange(object sender, GenericSocketStatusChageEventArgs args)
        {
            if (ConnectFeedback != null)
            {
                ConnectFeedback.FireUpdate();
            }

            if (StatusFeedback != null)
            {
                StatusFeedback.FireUpdate();
            }

            if (!args.Client.IsConnected && !_txQueue.IsEmpty)
            {
                CrestronInvoke.BeginInvoke(_ => _comms.Connect());
            }
        }

        private string GetHash(string randomNumber)
        {
            //response is of the form ntcontrol 1 {random}
            var randomString = randomNumber.Split(' ')[2];

            var stringToHash = String.Format("{0}:{1}:{2}", _config.Control.TcpSshProperties.Username,
                _config.Control.TcpSshProperties.Password, randomString);

            var bytes = Encoding.UTF8.GetBytes(stringToHash);

            var hash = _md5Provider.ComputeHash(bytes);

            return Encoding.UTF8.GetString(hash, 0, hash.Length);
        }

        private object DequeueAndSend(object notUsed)
        {

            if (_txQueue.IsEmpty)
            {
                Debug.Console(1, this, "Queue is empty we're out!");
                return null;
            }

            var cmdToSend = _txQueue.Dequeue(10);

            if (String.IsNullOrEmpty(cmdToSend))
            {
                Debug.Console(1, this, "Unable to get command to send");

            }

            _currentCommand = cmdToSend;
            _comms.SendText(String.IsNullOrEmpty(_hash) ? cmdToSend : String.Format("{0}{1}", _hash, cmdToSend));


            return null;
        }
        
        private void Handle_LineRecieved(object sender, GenericCommMethodReceiveTextArgs args)
        {
            _rxQueue.Enqueue(new QueueMessage(() => ParseResponse(args.Text)));
        }

        private void ParseResponse(string response)
        {
            //need to calculate hash
            if (response.ToLower().Contains("ntcontrol 1"))
            {
                _hash = GetHash(response);
                DequeueAndSend(null);
                return;
            }

            if (response.ToLower().Contains("ntcontrol 0"))
            {
                DequeueAndSend(null);
                return;
            }

            if (String.IsNullOrEmpty(_currentCommand))
            {
                return;
            }

            //power query
            if (_currentCommand.ToLower().Contains("qpw"))
            {
                if (_powerOnIgnoreFb && (response.Contains("001") || response.ToLower().Contains("pon")))
                {
                    _powerOnIgnoreFb = false;
                    return;
                }

                if (!(_powerOnIgnoreFb && (response.Contains("000") || response.ToLower().Contains("pof"))))
                {
                    PowerIsOn = response.Contains("001") || response.ToLower().Contains("pon");
                    return;
                }

                PowerIsOn = response.Contains("001") || response.ToLower().Contains("pon");
                return;
            }

            if (_currentCommand.ToLower().Contains("iis"))
            {
                CurrentInput = response.Replace("iis:", "").Trim();
            }
        }


        /// <summary>
        /// Sends text to the device plugin comms
        /// </summary>
        /// <remarks>
        /// Can be used to test commands with the device plugin using the DEVPROPS and DEVJSON console commands
        /// </remarks>
        /// <param name="text">Command to be sent</param>		
        public void SendText(string text)
        {
            if (_config.Control.Method == eControlMethod.Com)
            {
                _currentCommand = text;

                _comms.SendText(text);

                return;
            }

            if (_comms.IsConnected)
            {
                _currentCommand = text;
                _comms.SendText(String.IsNullOrEmpty(_hash) ? text : String.Format("{0}{1}", _hash, text));
            }
            else
            {

                _txQueue.Enqueue(text);
                Debug.Console(1, this, "Queue isn't empty and client isn't connected, connecting...");
                CrestronInvoke.BeginInvoke(_ => _comms.Connect());
            }
        }

        /// <summary>
        /// Connects/disconnects the comms of the plugin device
        /// </summary>
        /// <remarks>
        /// triggers the _comms.Connect/Disconnect as well as thee comms monitor start/stop
        /// </remarks>
        public bool Connect
        {
            get { return _comms.IsConnected; }
            set
            {
                if (value)
                {
                    _comms.Connect();
                    _commsMonitor.Start();
                }
                else
                {
                    _comms.Disconnect();
                    _commsMonitor.Stop();
                }
            }
        }

        public void Poll()
        {
            Debug.Console(1, this, "Sending poll...");
            SendText(_commandBuilder.GetCommand("QPW"));
        }        



        #region Power 

        public override void PowerOn()
        {
            if (PowerIsOn || _isWarming || _isCooling)
            {
                return;
            }

            _powerOnIgnoreFb = true;

            SendText(_commandBuilder.GetCommand("PON"));

            _isWarming = true;
            IsWarmingUpFeedback.FireUpdate();

            WarmupTimer = new CTimer(o =>
            {
                _isWarming = false;
                IsWarmingUpFeedback.FireUpdate();
                PowerIsOn = true;
            }, WarmupTime);
        }

        public override void PowerOff()
        {
            if (!PowerIsOn || _isWarming || _isCooling)
            {
                return;
            }

            SendText(_commandBuilder.GetCommand("POF"));

            _isCooling = true;
            IsCoolingDownFeedback.FireUpdate();

            CooldownTimer = new CTimer(o =>
            {
                _isCooling = false;
                PowerIsOn = false;
                IsCoolingDownFeedback.FireUpdate();
            }, CooldownTime);
        }

        public override void PowerToggle()
        {
            SendText(_commandBuilder.GetCommand(PowerIsOn ? "POF" : "PON"));
        }

        public bool PowerIsOn
        {
            get { return _powerIsOn; }
            set
            {
                Debug.Console(1, this, "Setting powerIsOn to {0} from {1}", value, _powerIsOn);

                if (value == _powerIsOn)
                {
                    return;
                }

                _powerIsOn = value;

                PowerIsOnFeedback.FireUpdate();
            }
        }

        protected override Func<bool> PowerIsOnFeedbackFunc
        {
            get
            {
                return () =>
                {
                    Debug.Console(1, this, "Updating PowerIsOnFeedback to {0}", PowerIsOn);
                    return PowerIsOn;
                };
            }
        }

        protected override Func<bool> IsCoolingDownFeedbackFunc
        {
            get { return () => _isCooling; }
        }

        protected override Func<bool> IsWarmingUpFeedbackFunc
        {
            get { return () => _isWarming; }
        }

        #endregion


        #region Input

#if SERIES4
        public ISelectableItems<byte> Inputs { get; private set; }


        //what I need to do, use existing logic that sets inputs and passes into an int value, and have that value select the
        //input from the list of ISelectableItems Inputs

        /* To-Do: Find the actual thing that sends the command.
         Build a class that satisfies Iselectable Items and an IselectableItems Class
            Each Input needs an implementation of Select and IsSelected, look at LG for examples
            ALL I NEED TO DO IS: when select is asserted on the input, it sends the command to the display
            when the main class parses the current selected input, it should iterate through my ISelectableItems and set them accordingly

        Create a separate file for the inputs class (see LG )
        The select method should just have an Action, and I can pass in the input method to be called
            */
#endif
        private void SetupInputPorts()
        {
#if SERIES4
            // 4-series logic
            Inputs = new PanasonicInputs()
            {
                Items = new Dictionary<byte, ISelectableItem>
                {
                    {
                        1, new PanasonicInput("1", "Computer 1", this, () => SetInput(eInputTypes.Rg1))
                    },
                    {
                        2, new PanasonicInput("2", "Computer 2", this, () => SetInput(eInputTypes.Rg2))
                    },
                    {
                        3, new PanasonicInput("3", "Video", this, () => SetInput(eInputTypes.Vid))
                    },
                    {
                        4, new PanasonicInput("4", "S-Video", this, () => SetInput(eInputTypes.Svd))
                    },
                    {
                        5, new PanasonicInput("5", "DVI", this, () => SetInput(eInputTypes.Dvi))
                    },
                    {
                        6, new PanasonicInput("6", "HDMI 1", this, () => SetInput(eInputTypes.Hd1))
                    },
                    {
                        7, new PanasonicInput("7", "HDMI 2", this, () => SetInput(eInputTypes.Hd2))
                    },
                    {
                        8, new PanasonicInput("8", "SDI", this, () => SetInput(eInputTypes.Sd1))
                    },
                    {
                        9, new PanasonicInput("9", "Digital Link", this, () => SetInput(eInputTypes.Dl1))
                    }
                }
            };
#else
			// 3-series logic
            var computer1 = new RoutingInputPort(RoutingPortNames.VgaIn, eRoutingSignalType.Video,
                eRoutingPortConnectionType.Vga, new Action(() => SetInput(eInputTypes.Rg1)), this);

            var computer2 = new RoutingInputPort(RoutingPortNames.VgaIn1, eRoutingSignalType.Video,
                eRoutingPortConnectionType.Vga, new Action(() => SetInput(eInputTypes.Rg2)), this);

            var video = new RoutingInputPort(RoutingPortNames.CompositeIn, eRoutingSignalType.Video,
                eRoutingPortConnectionType.Composite, new Action(() => SetInput(eInputTypes.Vid)), this);

            var sVideo = new RoutingInputPort(RoutingPortNames.ComponentIn, eRoutingSignalType.Video,
                eRoutingPortConnectionType.Component, new Action(() => SetInput(eInputTypes.Svd)), this);

            var dvi = new RoutingInputPort(RoutingPortNames.DviIn, eRoutingSignalType.Video,
                eRoutingPortConnectionType.Dvi, new Action(() => SetInput(eInputTypes.Dvi)), this);

            var hdmi1 = new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, new Action(() => SetInput(eInputTypes.Hd1)), this);

            var hdmi2 = new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, new Action(() => SetInput(eInputTypes.Hd2)), this);

            var sdi = new RoutingInputPort("Sdi", eRoutingSignalType.Video,
                eRoutingPortConnectionType.Sdi, new Action(() => SetInput(eInputTypes.Sd1)), this);

            var digitalLink = new RoutingInputPort(RoutingPortNames.DmIn, eRoutingSignalType.Video,
                eRoutingPortConnectionType.DmCat, new Action(() => SetInput(eInputTypes.Dl1)), this);


            InputPorts.Add(hdmi1);
            InputPorts.Add(dvi);
            InputPorts.Add(computer1);
            InputPorts.Add(computer2);
            InputPorts.Add(video);
            InputPorts.Add(sVideo);
            InputPorts.Add(hdmi2);
            InputPorts.Add(sdi);
            InputPorts.Add(digitalLink);
#endif
        }

        public void SetInput(eInputTypes input)
        {
            SendText(_commandBuilder.GetCommand("IIS", input.ToString().ToUpper()));

            CurrentInput = input.ToString();
        }

        public string CurrentInput
        {
            get { return _currentInput.ToString(); }
            set
            {
                if (_currentInput.ToString() == value)
                {
                    return;
                }

                try
                {
                    _currentInput = (eInputTypes)Enum.Parse(typeof(eInputTypes), value, true);
                }
                catch
                {
                    _currentInput = eInputTypes.None;
                }

                CurrentInputFeedback.FireUpdate();

#if SERIES4
                byte b = (byte)_currentInput;
                if (Inputs.Items.ContainsKey(b))
                {
                    Inputs.CurrentItem = b;
                }
                foreach (var item in Inputs.Items)
                {
                    item.Value.IsSelected = item.Key.Equals(b);
                }

                Inputs.CurrentItem = b;
#endif
            }
        }

        protected override Func<string> CurrentInputFeedbackFunc
        {
            get { return () => _currentInput.ToString(); }
        }

        /// <summary>
        /// Executes device switch
        /// </summary>
        /// <param name="selector"></param>
        public override void ExecuteSwitch(object selector)
        {
            if (PowerIsOn)
            {
                var handler = selector as Action;

                if (handler == null)
                {
                    Debug.Console(1, this, "Unable to switch using selector {0}", selector);
                    return;
                }

                handler();
            }
            else
            {
                EventHandler<FeedbackEventArgs> handler = null;
                var inputSelector = selector as Action;
                handler = (o, a) =>
                {
                    if (!_isWarming)
                    {
                        return;
                    }

                    IsWarmingUpFeedback.OutputChange -= handler;

                    if (inputSelector == null)
                    {
                        return;
                    }

                    inputSelector();
                };

                IsWarmingUpFeedback.OutputChange += handler;
                PowerOn();
            }
        }

        #endregion
    }
}