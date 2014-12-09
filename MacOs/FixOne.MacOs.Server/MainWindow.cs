using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using System.Diagnostics;
using System.Threading;
using FixOne.MacOs.Shared;

namespace FixOne.MacOs.Server
{
	public partial class MainWindow : MonoMac.AppKit.NSWindow
	{
		#region Constructors

		// Called when created from unmanaged code
		public MainWindow (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public MainWindow (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		// Shared initialization code
		void Initialize ()
		{
			this.Delegate = new MainWindowDelegate ();
			new Timer (delegate(object state) {
				BeginInvokeOnMainThread(() => {
					tblTable.ReloadData();
				});
			}, null, 5000, 5000);

		}

		#endregion

		[Export ("awakeFromNib:")]
		public override void AwakeFromNib()
		{
			tblTable.DataSource = new TableViewDataSource();
		}

	}
}

