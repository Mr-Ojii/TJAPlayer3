using System;
using System.Collections.Generic;
using System.Text;

namespace FDK
{
	public interface IInputDevice : IDisposable
	{
		// プロパティ

		EInputDeviceType eInputDeviceType
		{
			get;
		}
		string GUID 
		{
			get; 
		}
		int ID 
		{
			get;
		}
		List<STInputEvent> listInputEvents
		{
			get;
		}


		// メソッドインターフェース

		void tPolling( bool bIsWindowActive, bool bEnableBufferInput );
		bool bキーが押された( int nKey );
		bool bキーが押されている( int nKey );
		bool bキーが離された( int nKey );
		bool bキーが離されている( int nKey );
	}
}
