using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Plugins.Display.Panasonic.Projector
{
#if SERIES4
    public class PanasonicInputs : ISelectableItems<byte>
    {
        private Dictionary<byte, ISelectableItem> _items = new Dictionary<byte, ISelectableItem>();

        public Dictionary<byte, ISelectableItem> Items
        {
            get
            {
                return _items;
            }
            set
            {
                if (_items == value)
                    return;

                _items = value;

                ItemsUpdated?.Invoke(this, null);
            }
        }

        private byte _currentItem;

        public byte CurrentItem
        {
            get
            {
                return _currentItem;
            }
            set
            {
                if (_currentItem == value)
                    return;

                _currentItem = value;

                CurrentItemChanged?.Invoke(this, null);
            }
        }

        public event EventHandler ItemsUpdated;
        public event EventHandler CurrentItemChanged;

    }

    public class PanasonicInput : ISelectableItem
    {
        private bool _isSelected;

        private readonly PanasonicProjectorController _parent;

        private Action _inputMethod;

        public PanasonicInput(string key, string name, PanasonicProjectorController parent, Action inputMethod)
        {
            Key = key;
            Name = name;
            _parent = parent;
            _inputMethod = inputMethod;
        }

        public string Key { get; private set; }
        public string Name { get; private set; }

        public event EventHandler ItemUpdated;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected)
                    return;

                _isSelected = value;
                var handler = ItemUpdated;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        public void Select()
        {
            _inputMethod();
        }
    }
#endif
    }