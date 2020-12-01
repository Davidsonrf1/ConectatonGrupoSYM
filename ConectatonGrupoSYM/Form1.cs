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
			cl.FindPatientExtendido();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			var bundle = new RndsBundle();

			var id = "b01b7081ecc348c194e797d0399d83a2"; // Guid.NewGuid().ToString("N");

			bundle.BundleId = id;
			bundle.BundleIdValue = id;
			bundle.AuthorId = "00394544000185";
			bundle.PatientId = "700500572752652";

			bundle.ResultValueType = RndsResultValueType.Quantity;
			bundle.ResultQuantityValue = 2;
			bundle.ResultQualityValue = "";
			bundle.ReferenceRange = "1 - Detectável; 2 - Não detectável; 3 - Inconclusivo";
			bundle.SUSGroup = "0202";
			bundle.ExamCode = "94507-1";
			bundle.Method = "Imunoensaio enz.";


			var cl = new Hl7Client();

			cl.EnviaBundle(bundle);
		}
	}
}
