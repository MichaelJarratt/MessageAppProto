using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using MessageApp;

namespace MessageAppGUI
{
    public partial class PasswordPrompt : Form
    {
        private ManualResetEvent unblockMain; //this event is used to pause
        private bool submitted = false; //when form is closed, this is used to determine if user closed the window or submitted a password

        public PasswordPrompt(ManualResetEvent unblockMain)
        {
            InitializeComponent();
            this.unblockMain = unblockMain;
            CenterToScreen(); //puts itself in the centre of the screen, looks neater
        }
        //when user clicks "submit
        private void PasswordSubmit_Click(object sender, EventArgs e)
        {
            string password = PasswordTextbox.Text; //get entered password
            if (password.Length > 0)
            {
                Globals.setMasterKey(password); //generate globally accessible master key from password
                password = string.Empty; //clear from variable
                PasswordTextbox.Text = string.Empty; //I don't know if this is referenced or copied, but clear it to be sure
                unblockMain.Set(); //unblocks execution of main, message app form will be created
                submitted = true;
                this.Close();
            }
        }

        private void PasswordPrompt_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!submitted) //if user closed form without submitting password
            {
                PasswordTextbox.Text = string.Empty; //clear password
                Application.Exit(); //closes everything, as it couldn't run without the password. The main thread also would not be unblocked otherwise.
                
            }
        }
    }
}
