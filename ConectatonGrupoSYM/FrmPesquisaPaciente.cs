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
    public partial class FrmPesquisaPaciente : Form
    {
        public FrmPesquisaPaciente()
        {
            InitializeComponent();
        }

        private void BtnIncluir_Click(object sender, EventArgs e)
        {
            TxtResultado.Enabled = true;
            BtnEnviarResultado.Enabled = true;
        }
    }
}
