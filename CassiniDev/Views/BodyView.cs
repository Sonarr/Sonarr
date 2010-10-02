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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

#endregion

namespace CassiniDev.ServerLog
{
    [DefaultBindingProperty("Value")]
    public partial class BodyView : UserControl
    {
        private byte[] _value;

        public BodyView()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        public Byte[] Value
        {
            get { return _value; }
            set
            {
                _value = value;
                ClearDisplay();
                if (_value != null)
                {
                    HexViewTextBox.Text = _value.ConvertToHexView(8);
                    TextViewTextBox.Text = Encoding.UTF8.GetString(_value);
                    try
                    {
                        using (MemoryStream s = new MemoryStream(_value))
                        {
                            pictureBox1.Image = Image.FromStream(s);
                        }
                        pictureBox1.Visible = true;
                    }
                        // ReSharper disable EmptyGeneralCatchClause
                    catch
                        // ReSharper restore EmptyGeneralCatchClause
                    {
                    }
                }
            }
        }

        private void ClearDisplay()
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }
            HexViewTextBox.Text = "";
            TextViewTextBox.Text = "";
        }
    }
}