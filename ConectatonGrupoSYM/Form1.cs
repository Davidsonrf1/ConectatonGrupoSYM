using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConectatonGrupoSYM
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			var cl = new Hl7Client();

			cl.FindPatient("98222935003");
		}

		private void button2_Click(object sender, EventArgs e)
		{
			var bundle = new RndsBundle();

			bundle.BundleId = "123456";

			var cl = new Hl7Client();

			cl.EnviaBundle(bundle);
		}
	}
}
