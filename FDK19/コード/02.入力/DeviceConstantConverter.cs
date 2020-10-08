using System;
using System.Collections.Generic;
using System.Text;

using WindowsKey = System.Windows.Forms.Keys;
using SlimDXKey = SlimDXKeys.Key;

namespace FDK
{
	public class DeviceConstantConverter
	{
		// メソッド

		/// <returns>
		///		対応する値がなければ SlimDX.DirectInput.Unknown を返す。
		/// </returns>
		public static SlimDXKey TKKtoKey(OpenTK.Input.Key key)
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

		/// <returns>
		///		対応する値がなければ System.Windows.Forms.Keys.None を返す。
		/// </returns>
		public static WindowsKey KeyToKeys(SlimDXKey key)
		{
			if (_KeyToKeys.ContainsKey(key))
			{
				return _KeyToKeys[key];
			}
			else
			{
				return WindowsKey.None;
			}
		}

		/// <returns>
		///		対応する値がなければ SlimDX.DirectInput.Unknown を返す。
		/// </returns>
		public static SlimDXKey KeysToKey(WindowsKey key)
		{
			if (_KeysToKey.ContainsKey(key))
			{
				return _KeysToKey[key];
			}
			else
			{
				return SlimDXKey.Unknown;
			}
		}

		/// <summary>
		///		TKK (OpenTK.Input.Key) から SlimDX.DirectInput.Key への変換表。
		/// </summary>
		private static readonly Dictionary<OpenTK.Input.Key, SlimDXKey> _TKKtoKey = new Dictionary<OpenTK.Input.Key, SlimDXKey>() {
			#region [ *** ]
			{ OpenTK.Input.Key.Unknown, SlimDXKey.Unknown },
			{ OpenTK.Input.Key.ShiftLeft, SlimDXKey.LeftShift },
			{ OpenTK.Input.Key.ShiftRight, SlimDXKey.RightShift },
			{ OpenTK.Input.Key.ControlLeft, SlimDXKey.LeftControl },
			{ OpenTK.Input.Key.ControlRight, SlimDXKey.RightControl },
			{ OpenTK.Input.Key.AltLeft, SlimDXKey.LeftAlt },
			{ OpenTK.Input.Key.AltRight, SlimDXKey.RightAlt },
			{ OpenTK.Input.Key.WinLeft, SlimDXKey.LeftWindowsKey },
			{ OpenTK.Input.Key.WinRight, SlimDXKey.RightWindowsKey },
			{ OpenTK.Input.Key.F1, SlimDXKey.F1 },
			{ OpenTK.Input.Key.F2, SlimDXKey.F2 },
			{ OpenTK.Input.Key.F3, SlimDXKey.F3 },
			{ OpenTK.Input.Key.F4, SlimDXKey.F4 },
			{ OpenTK.Input.Key.F5, SlimDXKey.F5 },
			{ OpenTK.Input.Key.F6, SlimDXKey.F6 },
			{ OpenTK.Input.Key.F7, SlimDXKey.F7 },
			{ OpenTK.Input.Key.F8, SlimDXKey.F8 },
			{ OpenTK.Input.Key.F9, SlimDXKey.F9 },
			{ OpenTK.Input.Key.F10, SlimDXKey.F10 },
			{ OpenTK.Input.Key.F11, SlimDXKey.F11 },
			{ OpenTK.Input.Key.F12, SlimDXKey.F12 },
			{ OpenTK.Input.Key.F13, SlimDXKey.F13 },
			{ OpenTK.Input.Key.F14, SlimDXKey.F14 },
			{ OpenTK.Input.Key.F15, SlimDXKey.F15 },
			{ OpenTK.Input.Key.Up, SlimDXKey.UpArrow },
			{ OpenTK.Input.Key.Down, SlimDXKey.DownArrow },
			{ OpenTK.Input.Key.Left, SlimDXKey.LeftArrow },
			{ OpenTK.Input.Key.Right, SlimDXKey.RightArrow },
			{ OpenTK.Input.Key.Enter, SlimDXKey.Return },
			{ OpenTK.Input.Key.Escape, SlimDXKey.Escape },
			{ OpenTK.Input.Key.Space, SlimDXKey.Space },
			{ OpenTK.Input.Key.Tab, SlimDXKey.Tab },
			{ OpenTK.Input.Key.BackSpace, SlimDXKey.Backspace },
			{ OpenTK.Input.Key.Insert, SlimDXKey.Insert },
			{ OpenTK.Input.Key.Delete, SlimDXKey.Delete },
			{ OpenTK.Input.Key.PageUp, SlimDXKey.PageUp },
			{ OpenTK.Input.Key.PageDown, SlimDXKey.PageDown },
			{ OpenTK.Input.Key.Home, SlimDXKey.Home },
			{ OpenTK.Input.Key.End, SlimDXKey.End },
			{ OpenTK.Input.Key.CapsLock, SlimDXKey.CapsLock },
			{ OpenTK.Input.Key.ScrollLock, SlimDXKey.ScrollLock },
			{ OpenTK.Input.Key.PrintScreen, SlimDXKey.PrintScreen },
			{ OpenTK.Input.Key.Pause, SlimDXKey.Pause },
			{ OpenTK.Input.Key.NumLock, SlimDXKey.NumberLock },
			{ OpenTK.Input.Key.Sleep, SlimDXKey.Sleep },
			{ OpenTK.Input.Key.Keypad0, SlimDXKey.NumberPad0 },
			{ OpenTK.Input.Key.Keypad1, SlimDXKey.NumberPad1 },
			{ OpenTK.Input.Key.Keypad2, SlimDXKey.NumberPad2 },
			{ OpenTK.Input.Key.Keypad3, SlimDXKey.NumberPad3 },
			{ OpenTK.Input.Key.Keypad4, SlimDXKey.NumberPad4 },
			{ OpenTK.Input.Key.Keypad5, SlimDXKey.NumberPad5 },
			{ OpenTK.Input.Key.Keypad6, SlimDXKey.NumberPad6 },
			{ OpenTK.Input.Key.Keypad7, SlimDXKey.NumberPad7 },
			{ OpenTK.Input.Key.Keypad8, SlimDXKey.NumberPad8 },
			{ OpenTK.Input.Key.Keypad9, SlimDXKey.NumberPad9 },
			{ OpenTK.Input.Key.KeypadDivide, SlimDXKey.NumberPadSlash },
			{ OpenTK.Input.Key.KeypadMultiply, SlimDXKey.NumberPadStar },
			{ OpenTK.Input.Key.KeypadMinus, SlimDXKey.NumberPadMinus },
			{ OpenTK.Input.Key.KeypadPlus, SlimDXKey.NumberPadPlus },
			{ OpenTK.Input.Key.KeypadPeriod, SlimDXKey.NumberPadPeriod },
			{ OpenTK.Input.Key.KeypadEnter, SlimDXKey.NumberPadEnter },
			{ OpenTK.Input.Key.A, SlimDXKey.A },
			{ OpenTK.Input.Key.B, SlimDXKey.B },
			{ OpenTK.Input.Key.C, SlimDXKey.C },
			{ OpenTK.Input.Key.D, SlimDXKey.D },
			{ OpenTK.Input.Key.E, SlimDXKey.E },
			{ OpenTK.Input.Key.F, SlimDXKey.F },
			{ OpenTK.Input.Key.G, SlimDXKey.G },
			{ OpenTK.Input.Key.H, SlimDXKey.H },
			{ OpenTK.Input.Key.I, SlimDXKey.I },
			{ OpenTK.Input.Key.J, SlimDXKey.J },
			{ OpenTK.Input.Key.K, SlimDXKey.K },
			{ OpenTK.Input.Key.L, SlimDXKey.L },
			{ OpenTK.Input.Key.M, SlimDXKey.M },
			{ OpenTK.Input.Key.N, SlimDXKey.N },
			{ OpenTK.Input.Key.O, SlimDXKey.O },
			{ OpenTK.Input.Key.P, SlimDXKey.P },
			{ OpenTK.Input.Key.Q, SlimDXKey.Q },
			{ OpenTK.Input.Key.R, SlimDXKey.R },
			{ OpenTK.Input.Key.S, SlimDXKey.S },
			{ OpenTK.Input.Key.T, SlimDXKey.T },
			{ OpenTK.Input.Key.U, SlimDXKey.U },
			{ OpenTK.Input.Key.V, SlimDXKey.V },
			{ OpenTK.Input.Key.W, SlimDXKey.W },
			{ OpenTK.Input.Key.X, SlimDXKey.X },
			{ OpenTK.Input.Key.Y, SlimDXKey.Y },
			{ OpenTK.Input.Key.Z, SlimDXKey.Z },
			{ OpenTK.Input.Key.Number0, SlimDXKey.D0 },
			{ OpenTK.Input.Key.Number1, SlimDXKey.D1 },
			{ OpenTK.Input.Key.Number2, SlimDXKey.D2 },
			{ OpenTK.Input.Key.Number3, SlimDXKey.D3 },
			{ OpenTK.Input.Key.Number4, SlimDXKey.D4 },
			{ OpenTK.Input.Key.Number5, SlimDXKey.D5 },
			{ OpenTK.Input.Key.Number6, SlimDXKey.D6 },
			{ OpenTK.Input.Key.Number7, SlimDXKey.D7 },
			{ OpenTK.Input.Key.Number8, SlimDXKey.D8 },
			{ OpenTK.Input.Key.Number9, SlimDXKey.D9 },
			{ OpenTK.Input.Key.Grave, SlimDXKey.Grave },
			{ OpenTK.Input.Key.Minus, SlimDXKey.Minus },
			{ OpenTK.Input.Key.BracketLeft, SlimDXKey.LeftBracket },
			{ OpenTK.Input.Key.BracketRight, SlimDXKey.RightBracket },
			{ OpenTK.Input.Key.Semicolon, SlimDXKey.Semicolon },
			{ OpenTK.Input.Key.Quote, SlimDXKey.Apostrophe },
			{ OpenTK.Input.Key.Comma, SlimDXKey.Comma },
			{ OpenTK.Input.Key.Period, SlimDXKey.Period },
			{ OpenTK.Input.Key.Slash, SlimDXKey.Slash },
			{ OpenTK.Input.Key.BackSlash, SlimDXKey.Backslash },
			{ OpenTK.Input.Key.NonUSBackSlash, SlimDXKey.Yen },
			#endregion
		};

		/// <summary>
		///		SlimDX.DirectInput.Key から System.Windows.Form.Keys への変換表。
		/// </summary>
		private static readonly Dictionary<SlimDXKey, WindowsKey> _KeyToKeys = new Dictionary<SlimDXKey, WindowsKey>() {
			#region [ *** ]
			{ SlimDXKey.D0, WindowsKey.D0 },
			{ SlimDXKey.D1, WindowsKey.D1 },
			{ SlimDXKey.D2, WindowsKey.D2 },
			{ SlimDXKey.D3, WindowsKey.D3 },
			{ SlimDXKey.D4, WindowsKey.D4 },
			{ SlimDXKey.D5, WindowsKey.D5 },
			{ SlimDXKey.D6, WindowsKey.D6 },
			{ SlimDXKey.D7, WindowsKey.D7 },
			{ SlimDXKey.D8, WindowsKey.D8 },
			{ SlimDXKey.D9, WindowsKey.D9 },
			{ SlimDXKey.A, WindowsKey.A },
			{ SlimDXKey.B, WindowsKey.B },
			{ SlimDXKey.C, WindowsKey.C },
			{ SlimDXKey.D, WindowsKey.D },
			{ SlimDXKey.E, WindowsKey.E },
			{ SlimDXKey.F, WindowsKey.F },
			{ SlimDXKey.G, WindowsKey.G },
			{ SlimDXKey.H, WindowsKey.H },
			{ SlimDXKey.I, WindowsKey.I },
			{ SlimDXKey.J, WindowsKey.J },
			{ SlimDXKey.K, WindowsKey.K },
			{ SlimDXKey.L, WindowsKey.L },
			{ SlimDXKey.M, WindowsKey.M },
			{ SlimDXKey.N, WindowsKey.N },
			{ SlimDXKey.O, WindowsKey.O },
			{ SlimDXKey.P, WindowsKey.P },
			{ SlimDXKey.Q, WindowsKey.Q },
			{ SlimDXKey.R, WindowsKey.R },
			{ SlimDXKey.S, WindowsKey.S },
			{ SlimDXKey.T, WindowsKey.T },
			{ SlimDXKey.U, WindowsKey.U },
			{ SlimDXKey.V, WindowsKey.V },
			{ SlimDXKey.W, WindowsKey.W },
			{ SlimDXKey.X, WindowsKey.X },
			{ SlimDXKey.Y, WindowsKey.Y },
			{ SlimDXKey.Z, WindowsKey.Z },
			//{ SlimDXKey.AbntC1, WindowsKey.A },
			//{ SlimDXKey.AbntC2, WindowsKey.A },
			{ SlimDXKey.Apostrophe, WindowsKey.OemQuotes },
			{ SlimDXKey.Applications, WindowsKey.Apps },
			{ SlimDXKey.AT, WindowsKey.Oem3 },	// OemTilde と同値
			//{ SlimDXKey.AX, WindowsKey.A },	// OemAX (225) は未定義
			{ SlimDXKey.Backspace, WindowsKey.Back },
			{ SlimDXKey.Backslash, WindowsKey.OemBackslash },
			//{ SlimDXKey.Calculator, WindowsKey.A },
			{ SlimDXKey.CapsLock, WindowsKey.CapsLock },
			{ SlimDXKey.Colon, WindowsKey.Oem1 },
			{ SlimDXKey.Comma, WindowsKey.Oemcomma },
			{ SlimDXKey.Convert, WindowsKey.IMEConvert },
			{ SlimDXKey.Delete, WindowsKey.Delete },
			{ SlimDXKey.DownArrow, WindowsKey.Down },
			{ SlimDXKey.End, WindowsKey.End },
			{ SlimDXKey.Equals, WindowsKey.A },		// ?
			{ SlimDXKey.Escape, WindowsKey.Escape },
			{ SlimDXKey.F1, WindowsKey.F1 },
			{ SlimDXKey.F2, WindowsKey.F2 },
			{ SlimDXKey.F3, WindowsKey.F3 },
			{ SlimDXKey.F4, WindowsKey.F4 },
			{ SlimDXKey.F5, WindowsKey.F5 },
			{ SlimDXKey.F6, WindowsKey.F6 },
			{ SlimDXKey.F7, WindowsKey.F7 },
			{ SlimDXKey.F8, WindowsKey.F8 },
			{ SlimDXKey.F9, WindowsKey.F9 },
			{ SlimDXKey.F10, WindowsKey.F10 },
			{ SlimDXKey.F11, WindowsKey.F11 },
			{ SlimDXKey.F12, WindowsKey.F12 },
			{ SlimDXKey.F13, WindowsKey.F13 },
			{ SlimDXKey.F14, WindowsKey.F14 },
			{ SlimDXKey.F15, WindowsKey.F15 },
			{ SlimDXKey.Grave, WindowsKey.A },		// ?
			{ SlimDXKey.Home, WindowsKey.Home },
			{ SlimDXKey.Insert, WindowsKey.Insert },
			{ SlimDXKey.Kana, WindowsKey.KanaMode },
			{ SlimDXKey.Kanji, WindowsKey.KanjiMode },
			{ SlimDXKey.LeftBracket, WindowsKey.Oem4 },
			{ SlimDXKey.LeftControl, WindowsKey.LControlKey },
			{ SlimDXKey.LeftArrow, WindowsKey.Left },
			{ SlimDXKey.LeftAlt, WindowsKey.LMenu },
			{ SlimDXKey.LeftShift, WindowsKey.LShiftKey },
			{ SlimDXKey.LeftWindowsKey, WindowsKey.LWin },
			{ SlimDXKey.Mail, WindowsKey.LaunchMail },
			{ SlimDXKey.MediaSelect, WindowsKey.SelectMedia },
			{ SlimDXKey.MediaStop, WindowsKey.MediaStop },
			{ SlimDXKey.Minus, WindowsKey.OemMinus },
			{ SlimDXKey.Mute, WindowsKey.VolumeMute },
			{ SlimDXKey.MyComputer, WindowsKey.A },		// ?
			{ SlimDXKey.NextTrack, WindowsKey.MediaNextTrack },
			{ SlimDXKey.NoConvert, WindowsKey.IMENonconvert },
			{ SlimDXKey.NumberLock, WindowsKey.NumLock },
			{ SlimDXKey.NumberPad0, WindowsKey.NumPad0 },
			{ SlimDXKey.NumberPad1, WindowsKey.NumPad1 },
			{ SlimDXKey.NumberPad2, WindowsKey.NumPad2 },
			{ SlimDXKey.NumberPad3, WindowsKey.NumPad3 },
			{ SlimDXKey.NumberPad4, WindowsKey.NumPad4 },
			{ SlimDXKey.NumberPad5, WindowsKey.NumPad5 },
			{ SlimDXKey.NumberPad6, WindowsKey.NumPad6 },
			{ SlimDXKey.NumberPad7, WindowsKey.NumPad7 },
			{ SlimDXKey.NumberPad8, WindowsKey.NumPad8 },
			{ SlimDXKey.NumberPad9, WindowsKey.NumPad9 },
			{ SlimDXKey.NumberPadComma, WindowsKey.Separator },
			{ SlimDXKey.NumberPadEnter, WindowsKey.A },		// ?
			{ SlimDXKey.NumberPadEquals, WindowsKey.A },		// ?
			{ SlimDXKey.NumberPadMinus, WindowsKey.Subtract },
			{ SlimDXKey.NumberPadPeriod, WindowsKey.Decimal },
			{ SlimDXKey.NumberPadPlus, WindowsKey.Add },
			{ SlimDXKey.NumberPadSlash, WindowsKey.Divide },
			{ SlimDXKey.NumberPadStar, WindowsKey.Multiply },
			{ SlimDXKey.Oem102, WindowsKey.Oem102 },
			{ SlimDXKey.PageDown, WindowsKey.PageDown },
			{ SlimDXKey.PageUp, WindowsKey.PageUp },
			{ SlimDXKey.Pause, WindowsKey.Pause },
			{ SlimDXKey.Period, WindowsKey.OemPeriod },
			{ SlimDXKey.PlayPause, WindowsKey.MediaPlayPause },
			{ SlimDXKey.Power, WindowsKey.A },		// ?
			{ SlimDXKey.PreviousTrack, WindowsKey.MediaPreviousTrack },
			{ SlimDXKey.RightBracket, WindowsKey.Oem6 },
			{ SlimDXKey.RightControl, WindowsKey.RControlKey },
			{ SlimDXKey.Return, WindowsKey.Return },
			{ SlimDXKey.RightArrow, WindowsKey.Right },
			{ SlimDXKey.RightAlt, WindowsKey.RMenu },
			{ SlimDXKey.RightShift, WindowsKey.A },		// ?
			{ SlimDXKey.RightWindowsKey, WindowsKey.RWin },
			{ SlimDXKey.ScrollLock, WindowsKey.Scroll },
			{ SlimDXKey.Semicolon, WindowsKey.Oemplus    },	// OemSemicolon じゃなくて？
			{ SlimDXKey.Slash, WindowsKey.Oem2 },
			{ SlimDXKey.Sleep, WindowsKey.Sleep },
			{ SlimDXKey.Space, WindowsKey.Space },
			{ SlimDXKey.Stop, WindowsKey.MediaStop },
			{ SlimDXKey.PrintScreen, WindowsKey.PrintScreen },
			{ SlimDXKey.Tab, WindowsKey.Tab },
			{ SlimDXKey.Underline, WindowsKey.Oem102 },
			//{ SlimDXKey.Unlabeled, WindowsKey.A },		// ?
			{ SlimDXKey.UpArrow, WindowsKey.Up },
			{ SlimDXKey.VolumeDown, WindowsKey.VolumeDown },
			{ SlimDXKey.VolumeUp, WindowsKey.VolumeUp },
			{ SlimDXKey.Wake, WindowsKey.A },		// ?
			{ SlimDXKey.WebBack, WindowsKey.BrowserBack },
			{ SlimDXKey.WebFavorites, WindowsKey.BrowserFavorites },
			{ SlimDXKey.WebForward, WindowsKey.BrowserForward },
			{ SlimDXKey.WebHome, WindowsKey.BrowserHome },
			{ SlimDXKey.WebRefresh, WindowsKey.BrowserRefresh },
			{ SlimDXKey.WebSearch, WindowsKey.BrowserSearch },
			{ SlimDXKey.WebStop, WindowsKey.BrowserStop },
			{ SlimDXKey.Yen, WindowsKey.OemBackslash },
			#endregion
		};
		/// <summary>
		///		System.Windows.Form.Keys から SlimDX.DirectInput.Key への変換表。
		/// </summary>
		private static readonly Dictionary<WindowsKey, SlimDXKey> _KeysToKey = new Dictionary<WindowsKey, SlimDXKey>() {
			#region [ *** ]
			{ WindowsKey.D0,SlimDXKey.D0 },
			{ WindowsKey.D1,SlimDXKey.D1 },
			{ WindowsKey.D2,SlimDXKey.D2 },
			{ WindowsKey.D3,SlimDXKey.D3 },
			{ WindowsKey.D4,SlimDXKey.D4 },
			{ WindowsKey.D5,SlimDXKey.D5 },
			{ WindowsKey.D6,SlimDXKey.D6 },
			{ WindowsKey.D7,SlimDXKey.D7 },
			{ WindowsKey.D8,SlimDXKey.D8 },
			{ WindowsKey.D9,SlimDXKey.D9 },
			{ WindowsKey.A, SlimDXKey.A },
			{ WindowsKey.B, SlimDXKey.B },
			{ WindowsKey.C, SlimDXKey.C },
			{ WindowsKey.D, SlimDXKey.D },
			{ WindowsKey.E, SlimDXKey.E },
			{ WindowsKey.F, SlimDXKey.F },
			{ WindowsKey.G, SlimDXKey.G },
			{ WindowsKey.H, SlimDXKey.H },
			{ WindowsKey.I, SlimDXKey.I },
			{ WindowsKey.J, SlimDXKey.J },
			{ WindowsKey.K, SlimDXKey.K },
			{ WindowsKey.L, SlimDXKey.L },
			{ WindowsKey.M, SlimDXKey.M },
			{ WindowsKey.N, SlimDXKey.N },
			{ WindowsKey.O, SlimDXKey.O },
			{ WindowsKey.P, SlimDXKey.P },
			{ WindowsKey.Q, SlimDXKey.Q },
			{ WindowsKey.R, SlimDXKey.R },
			{ WindowsKey.S, SlimDXKey.S },
			{ WindowsKey.T, SlimDXKey.T },
			{ WindowsKey.U, SlimDXKey.U },
			{ WindowsKey.V, SlimDXKey.V },
			{ WindowsKey.W, SlimDXKey.W },
			{ WindowsKey.X, SlimDXKey.X },
			{ WindowsKey.Y, SlimDXKey.Y },
			{ WindowsKey.Z, SlimDXKey.Z },
			{ WindowsKey.F1, SlimDXKey.F1 },
			{ WindowsKey.F2, SlimDXKey.F2 },
			{ WindowsKey.F3, SlimDXKey.F3 },
			{ WindowsKey.F4, SlimDXKey.F4 },
			{ WindowsKey.F5, SlimDXKey.F5 },
			{ WindowsKey.F6, SlimDXKey.F6 },
			{ WindowsKey.F7, SlimDXKey.F7 },
			{ WindowsKey.F8, SlimDXKey.F8 },
			{ WindowsKey.F9, SlimDXKey.F9 },
			{ WindowsKey.F10, SlimDXKey.F10 },
			{ WindowsKey.F11, SlimDXKey.F11 },
			{ WindowsKey.F12, SlimDXKey.F12 },
			{ WindowsKey.F13, SlimDXKey.F13 },
			{ WindowsKey.F14, SlimDXKey.F14 },
			{ WindowsKey.F15, SlimDXKey.F15 },
			{ WindowsKey.NumLock, SlimDXKey.NumberLock },
			{ WindowsKey.NumPad0, SlimDXKey.NumberPad0 },
			{ WindowsKey.NumPad1, SlimDXKey.NumberPad1 },
			{ WindowsKey.NumPad2, SlimDXKey.NumberPad2 },
			{ WindowsKey.NumPad3, SlimDXKey.NumberPad3 },
			{ WindowsKey.NumPad4, SlimDXKey.NumberPad4 },
			{ WindowsKey.NumPad5, SlimDXKey.NumberPad5 },
			{ WindowsKey.NumPad6, SlimDXKey.NumberPad6 },
			{ WindowsKey.NumPad7, SlimDXKey.NumberPad7 },
			{ WindowsKey.NumPad8, SlimDXKey.NumberPad8 },
			{ WindowsKey.NumPad9, SlimDXKey.NumberPad9 },
			{ WindowsKey.OemQuotes,SlimDXKey.Apostrophe },
			{ WindowsKey.Apps,SlimDXKey.Applications },
			{ WindowsKey.Oem3,SlimDXKey.AT },
			{ WindowsKey.Back,SlimDXKey.Backspace },
			{ WindowsKey.OemBackslash, SlimDXKey.Backslash },
			{ WindowsKey.CapsLock,SlimDXKey.CapsLock },
			{ WindowsKey.Oem1,SlimDXKey.Colon },
			{ WindowsKey.Oemcomma,SlimDXKey.Comma },
			{ WindowsKey.IMEConvert,SlimDXKey.Convert },
			{ WindowsKey.Delete,SlimDXKey.Delete },
			{ WindowsKey.Down,SlimDXKey.DownArrow },
			{ WindowsKey.End,SlimDXKey.End },
			{ WindowsKey.Escape,SlimDXKey.Escape },
			{ WindowsKey.Home, SlimDXKey.Home },
			{ WindowsKey.Insert, SlimDXKey.Insert },
			{ WindowsKey.KanaMode, SlimDXKey.Kana },
			{ WindowsKey.KanjiMode, SlimDXKey.Kanji },
			{ WindowsKey.Oem4, SlimDXKey.LeftBracket },
			{ WindowsKey.Control, SlimDXKey.LeftControl },
			{ WindowsKey.Alt, SlimDXKey.LeftAlt },
			{ WindowsKey.ShiftKey,SlimDXKey.LeftShift },
			{ WindowsKey.ControlKey,SlimDXKey.LeftControl },
			{ WindowsKey.LControlKey, SlimDXKey.LeftControl },
			{ WindowsKey.Left, SlimDXKey.LeftArrow },
			{ WindowsKey.LMenu,SlimDXKey.LeftAlt },
			{ WindowsKey.LShiftKey,SlimDXKey.LeftShift },
			{ WindowsKey.LWin, SlimDXKey.LeftWindowsKey },
			{ WindowsKey.LaunchMail, SlimDXKey.Mail },
			{ WindowsKey.SelectMedia,SlimDXKey.MediaSelect },
			{ WindowsKey.MediaStop, SlimDXKey.MediaStop },
			{ WindowsKey.OemMinus, SlimDXKey.Minus },
			{ WindowsKey.VolumeMute, SlimDXKey.Mute },
			{ WindowsKey.MediaNextTrack, SlimDXKey.NextTrack },
			{ WindowsKey.IMENonconvert, SlimDXKey.NoConvert },
			{ WindowsKey.Separator, SlimDXKey.NumberPadComma },
			{ WindowsKey.Subtract, SlimDXKey.NumberPadMinus },
			{ WindowsKey.Decimal, SlimDXKey.NumberPadPeriod },
			{ WindowsKey.Add,SlimDXKey.NumberPadPlus },
			{ WindowsKey.Divide, SlimDXKey.NumberPadSlash },
			{ WindowsKey.Multiply, SlimDXKey.NumberPadStar },
			{ WindowsKey.PageDown, SlimDXKey.PageDown },
			{ WindowsKey.PageUp, SlimDXKey.PageUp },
			{ WindowsKey.Pause, SlimDXKey.Pause },
			{ WindowsKey.OemPeriod, SlimDXKey.Period },
			{ WindowsKey.MediaPlayPause, SlimDXKey.PlayPause },
			{ WindowsKey.MediaPreviousTrack, SlimDXKey.PreviousTrack },
			{ WindowsKey.Oem6, SlimDXKey.RightBracket },
			{ WindowsKey.RControlKey, SlimDXKey.RightControl },
			{ WindowsKey.Return, SlimDXKey.Return },
			{ WindowsKey.Right, SlimDXKey.RightArrow },
			{ WindowsKey.RMenu, SlimDXKey.RightAlt },
			{ WindowsKey.RWin, SlimDXKey.RightWindowsKey },
			{ WindowsKey.Scroll, SlimDXKey.ScrollLock },
			{ WindowsKey.Oem2, SlimDXKey.Slash },
			{ WindowsKey.Sleep, SlimDXKey.Sleep },
			{ WindowsKey.Space, SlimDXKey.Space },
			{ WindowsKey.PrintScreen, SlimDXKey.PrintScreen },
			{ WindowsKey.Tab, SlimDXKey.Tab },
			{ WindowsKey.Up, SlimDXKey.UpArrow },
			{ WindowsKey.VolumeDown, SlimDXKey.VolumeDown },
			{ WindowsKey.VolumeUp, SlimDXKey.VolumeUp },
			{ WindowsKey.BrowserBack, SlimDXKey.WebBack },
			{ WindowsKey.BrowserFavorites, SlimDXKey.WebFavorites },
			{ WindowsKey.BrowserForward, SlimDXKey.WebForward },
			{ WindowsKey.BrowserHome, SlimDXKey.WebHome },
			{ WindowsKey.BrowserRefresh, SlimDXKey.WebRefresh },
			{ WindowsKey.BrowserSearch, SlimDXKey.WebSearch },
			{ WindowsKey.BrowserStop, SlimDXKey.WebStop },
			#endregion
		};
	}
}
