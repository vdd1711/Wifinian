﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Wifinian.Views.Controls
{
	public class SwitchTextBox : TextBox
	{
		private readonly DispatcherTimer _timer;

		public TimeSpan HoldingDuration { get; set; } = TimeSpan.FromSeconds(1.2);

		public SwitchTextBox() : base()
		{
			_timer = new DispatcherTimer();
			_timer.Tick += OnTick;

			this.PreviewMouseLeftButtonDown += (sender, e) => OnDeviceDown(e.MouseDevice, true);
			this.PreviewMouseRightButtonDown += (sender, e) => OnDeviceDown(e.MouseDevice, false);
			this.PreviewStylusDown += (sender, e) => OnDeviceDown(e.StylusDevice, false);
			this.PreviewTouchDown += (sender, e) => OnDeviceDown(e.TouchDevice, false);

			this.PreviewMouseUp += (sender, e) => OnDeviceUp();
			this.PreviewStylusUp += (sender, e) => OnDeviceUp();
			this.PreviewTouchUp += (sender, e) => OnDeviceUp();
			this.MouseLeave += (sender, e) => OnDeviceUp();
			this.StylusLeave += (sender, e) => OnDeviceUp();
			this.TouchLeave += (sender, e) => OnDeviceUp();
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			this.IsReadOnly = true;
		}

		private const double Tolerance = 10D;
		private InputDevice _device;
		private Point _startPosition;
		private bool _isContextMenuOpenable = true;

		private void OnDeviceDown(InputDevice device, bool isContextMenuOpenable)
		{
			if (!this.IsReadOnly)
				return;

			this._device = device;
			if (!TryGetDevicePosition(this._device, out _startPosition))
				return;

			this._isContextMenuOpenable = isContextMenuOpenable;

			_timer.Interval = HoldingDuration;
			_timer.Start();
		}

		private void OnDeviceUp()
		{
			_timer.Stop();

			_device = null;
		}

		private void OnTick(object sender, EventArgs e)
		{
			_timer.Stop();

			if (!TryGetDevicePosition(_device, out Point endPosition))
				return;

			if (new Vector(endPosition.X - _startPosition.X, endPosition.Y - _startPosition.Y).Length > Tolerance)
				return;

			this.IsReadOnly = false;

			// Get focus.
			var window = Window.GetWindow(this);
			FocusManager.SetFocusedElement(window, this);
			Keyboard.Focus(this);
			this.SelectionStart = 0;
		}

		private bool TryGetDevicePosition(InputDevice device, out Point position)
		{
			switch (device)
			{
				case MouseDevice mouse:
					position = mouse.GetPosition(this);
					return true;

				case StylusDevice stylus:
					position = stylus.GetPosition(this);
					return true;

				case TouchDevice touch:
					position = touch.GetTouchPoint(this).Position;
					return true;

				default:
					position = default(Point);
					return false;
			}
		}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			_timer.Stop();

			this.IsReadOnly = true;

			base.OnLostFocus(e);
		}

		protected override void OnContextMenuOpening(ContextMenuEventArgs e)
		{
			if (_isContextMenuOpenable)
			{
				base.OnContextMenuOpening(e);
			}
			_isContextMenuOpenable = true;
		}
	}
}