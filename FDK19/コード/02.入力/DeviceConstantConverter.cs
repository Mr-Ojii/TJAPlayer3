using System;
using System.Collections.Generic;
using System.Text;

using SlimDXKey = SlimDXKeys.Key;
using TKKey = OpenTK.Input.Key;

namespace FDK
{
	public class DeviceConstantConverter
	{
		// メソッド

		/// <returns>
		///		対応する値がなければ SlimDX.DirectInput.Unknown を返す。
		/// </returns>
		public static SlimDXKey TKKtoKey(TKKey key)
		{
			if (_TKKtoKey.ContainsKey(key))
			{
				return _TKKtoKey[key];
			}
			else
			{
				return SlimDXKey.Unknown;
			}
		}

		/// <summary>
		///		TKK (TKKey) から SlimDX.DirectInput.Key への変換表。
		/// </summary>
		private static readonly Dictionary<TKKey, SlimDXKey> _TKKtoKey = new Dictionary<TKKey, SlimDXKey>() {
			#region [ *** ]
			{ TKKey.Unknown, SlimDXKey.Unknown },
			{ TKKey.ShiftLeft, SlimDXKey.LeftShift },
			{ TKKey.ShiftRight, SlimDXKey.RightShift },
			{ TKKey.ControlLeft, SlimDXKey.LeftControl },
			{ TKKey.ControlRight, SlimDXKey.RightControl },
			{ TKKey.AltLeft, SlimDXKey.LeftAlt },
			{ TKKey.AltRight, SlimDXKey.RightAlt },
			{ TKKey.WinLeft, SlimDXKey.LeftWindowsKey },
			{ TKKey.WinRight, SlimDXKey.RightWindowsKey },
			{ TKKey.F1, SlimDXKey.F1 },
			{ TKKey.F2, SlimDXKey.F2 },
			{ TKKey.F3, SlimDXKey.F3 },
			{ TKKey.F4, SlimDXKey.F4 },
			{ TKKey.F5, SlimDXKey.F5 },
			{ TKKey.F6, SlimDXKey.F6 },
			{ TKKey.F7, SlimDXKey.F7 },
			{ TKKey.F8, SlimDXKey.F8 },
			{ TKKey.F9, SlimDXKey.F9 },
			{ TKKey.F10, SlimDXKey.F10 },
			{ TKKey.F11, SlimDXKey.F11 },
			{ TKKey.F12, SlimDXKey.F12 },
			{ TKKey.F13, SlimDXKey.F13 },
			{ TKKey.F14, SlimDXKey.F14 },
			{ TKKey.F15, SlimDXKey.F15 },
			{ TKKey.Up, SlimDXKey.UpArrow },
			{ TKKey.Down, SlimDXKey.DownArrow },
			{ TKKey.Left, SlimDXKey.LeftArrow },
			{ TKKey.Right, SlimDXKey.RightArrow },
			{ TKKey.Enter, SlimDXKey.Return },
			{ TKKey.Escape, SlimDXKey.Escape },
			{ TKKey.Space, SlimDXKey.Space },
			{ TKKey.Tab, SlimDXKey.Tab },
			{ TKKey.BackSpace, SlimDXKey.Backspace },
			{ TKKey.Insert, SlimDXKey.Insert },
			{ TKKey.Delete, SlimDXKey.Delete },
			{ TKKey.PageUp, SlimDXKey.PageUp },
			{ TKKey.PageDown, SlimDXKey.PageDown },
			{ TKKey.Home, SlimDXKey.Home },
			{ TKKey.End, SlimDXKey.End },
			{ TKKey.CapsLock, SlimDXKey.CapsLock },
			{ TKKey.ScrollLock, SlimDXKey.ScrollLock },
			{ TKKey.PrintScreen, SlimDXKey.PrintScreen },
			{ TKKey.Pause, SlimDXKey.Pause },
			{ TKKey.NumLock, SlimDXKey.NumberLock },
			{ TKKey.Sleep, SlimDXKey.Sleep },
			{ TKKey.Keypad0, SlimDXKey.NumberPad0 },
			{ TKKey.Keypad1, SlimDXKey.NumberPad1 },
			{ TKKey.Keypad2, SlimDXKey.NumberPad2 },
			{ TKKey.Keypad3, SlimDXKey.NumberPad3 },
			{ TKKey.Keypad4, SlimDXKey.NumberPad4 },
			{ TKKey.Keypad5, SlimDXKey.NumberPad5 },
			{ TKKey.Keypad6, SlimDXKey.NumberPad6 },
			{ TKKey.Keypad7, SlimDXKey.NumberPad7 },
			{ TKKey.Keypad8, SlimDXKey.NumberPad8 },
			{ TKKey.Keypad9, SlimDXKey.NumberPad9 },
			{ TKKey.KeypadDivide, SlimDXKey.NumberPadSlash },
			{ TKKey.KeypadMultiply, SlimDXKey.NumberPadStar },
			{ TKKey.KeypadMinus, SlimDXKey.NumberPadMinus },
			{ TKKey.KeypadPlus, SlimDXKey.NumberPadPlus },
			{ TKKey.KeypadPeriod, SlimDXKey.NumberPadPeriod },
			{ TKKey.KeypadEnter, SlimDXKey.NumberPadEnter },
			{ TKKey.A, SlimDXKey.A },
			{ TKKey.B, SlimDXKey.B },
			{ TKKey.C, SlimDXKey.C },
			{ TKKey.D, SlimDXKey.D },
			{ TKKey.E, SlimDXKey.E },
			{ TKKey.F, SlimDXKey.F },
			{ TKKey.G, SlimDXKey.G },
			{ TKKey.H, SlimDXKey.H },
			{ TKKey.I, SlimDXKey.I },
			{ TKKey.J, SlimDXKey.J },
			{ TKKey.K, SlimDXKey.K },
			{ TKKey.L, SlimDXKey.L },
			{ TKKey.M, SlimDXKey.M },
			{ TKKey.N, SlimDXKey.N },
			{ TKKey.O, SlimDXKey.O },
			{ TKKey.P, SlimDXKey.P },
			{ TKKey.Q, SlimDXKey.Q },
			{ TKKey.R, SlimDXKey.R },
			{ TKKey.S, SlimDXKey.S },
			{ TKKey.T, SlimDXKey.T },
			{ TKKey.U, SlimDXKey.U },
			{ TKKey.V, SlimDXKey.V },
			{ TKKey.W, SlimDXKey.W },
			{ TKKey.X, SlimDXKey.X },
			{ TKKey.Y, SlimDXKey.Y },
			{ TKKey.Z, SlimDXKey.Z },
			{ TKKey.Number0, SlimDXKey.D0 },
			{ TKKey.Number1, SlimDXKey.D1 },
			{ TKKey.Number2, SlimDXKey.D2 },
			{ TKKey.Number3, SlimDXKey.D3 },
			{ TKKey.Number4, SlimDXKey.D4 },
			{ TKKey.Number5, SlimDXKey.D5 },
			{ TKKey.Number6, SlimDXKey.D6 },
			{ TKKey.Number7, SlimDXKey.D7 },
			{ TKKey.Number8, SlimDXKey.D8 },
			{ TKKey.Number9, SlimDXKey.D9 },
			{ TKKey.Grave, SlimDXKey.Grave },
			{ TKKey.Minus, SlimDXKey.Minus },
			{ TKKey.BracketLeft, SlimDXKey.LeftBracket },
			{ TKKey.BracketRight, SlimDXKey.RightBracket },
			{ TKKey.Semicolon, SlimDXKey.Semicolon },
			{ TKKey.Quote, SlimDXKey.Apostrophe },
			{ TKKey.Comma, SlimDXKey.Comma },
			{ TKKey.Period, SlimDXKey.Period },
			{ TKKey.Slash, SlimDXKey.Slash },
			{ TKKey.BackSlash, SlimDXKey.Backslash },
			{ TKKey.NonUSBackSlash, SlimDXKey.Yen },
			#endregion
		};
	}
}
