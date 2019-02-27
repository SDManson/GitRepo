using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nexmo.Api;
using System.Net.Mail;
using System.Net;
//using EASendMail;


namespace Texter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MailMessage msg = new MailMessage("sdmanson@gmail.com", "9034158375@txt.att.net", "Client Crashed", "Sanderling has crashed");
            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("sdmanson@gmail.com", "StudliestOne");
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.Timeout = 20000;
            client.Send(msg);
        }
    }
}
