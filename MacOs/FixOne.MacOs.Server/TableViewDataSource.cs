using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace FixOne.MacOs.Server
{
	[Register ("TableViewDataSource")]
	public class TableViewDataSource : NSTableViewDataSource
	{
		public TableViewDataSource ()
		{
		}

		// This method will be called by the NSTableView control to learn the number of rows to display.
		[Export ("numberOfRowsInTableView:")]
		public int NumberOfRowsInTableView(NSTableView table)
		{
			return EngineController.Log.Count;
		}

		// This method will be called by the control for each column and each row.
		[Export ("tableView:objectValueForTableColumn:row:")]
		public NSObject ObjectValueForTableColumn(NSTableView table, NSTableColumn col, int row)
		{
			return EngineController.Log[row];
		}
	}//class
}

