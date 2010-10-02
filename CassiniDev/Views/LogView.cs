//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) Sky Sanders. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.htm file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace CassiniDev.ServerLog
{
    /// <summary>
    /// If log can persist, then only stack values in the list and tag the items with the RowId for querying the db for heavy values.
    /// If not, tag the item with the loginfo instance. Memory usage will be much greater but is better than nothing.
    /// </summary>
    public partial class LogView : Form
    {

        

        private Server _server;

        public LogView(Server server)
        {
            InitializeComponent();
            _server = server;
            _server.RequestComplete += RequestComplete;
            

            InitializeList();

            base.Text = SR.GetString(SR.WebdevLogViewerNameWithPort, _server.Port);
        }

        /// <summary>
        /// Not sure if these qualify for disposable
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            _server.RequestComplete -= RequestComplete;
            _server = null;
            
        }

        private void AddLogRows(IEnumerable<LogInfo> items)
        {
            listView1.SuspendLayout();

            foreach (LogInfo item in items)
            {
                ListViewItem a = new ListViewItem(new[]
                    {
                        item.RowType == 0 ? "" : item.RowType == 1 ? "Request" : "Response",
                        item.Created.ToString(),
                        item.StatusCode.ToString(),
                        item.Url,
                        item.PathTranslated,
                        item.Identity
                    })
                    {
                        Tag = item
                    };
                listView1.Items.Add(a);
            }

            if (listView1.Items.Count > 0 && scrollLogToolStripMenuItem.Checked)
            {
                int lastRow = listView1.Items.Count - 1;
                listView1.EnsureVisible(lastRow);
            }
            listView1.ResumeLayout();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitializeList();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private LogInfo GetSelectedLogItem()
        {
            object tag = listView1.Items[listView1.SelectedIndices[0]].Tag;

            LogInfo returnValue = (LogInfo)tag;

            return returnValue;
        }

        private void InitializeList()
        {
            listView1.SuspendLayout();
            listView1.Items.Clear();
            listView1.ResumeLayout();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                LogInfo log = GetSelectedLogItem();
                exceptionRichTextBox.Text = log.Exception;
                headersRichTextBox.Text = log.Headers;
                bodyBodyView.Value = log.Body;
            }
            else
            {
                exceptionRichTextBox.Text = "";
                headersRichTextBox.Text = "";
                bodyBodyView.Value = null;
            }
        }

        private void RequestComplete(object sender, RequestEventArgs e)
        {
            Invoke(new MethodInvoker(() => AddLogRows(new[] { e.RequestLog, e.ResponseLog })));
        }

    }
}