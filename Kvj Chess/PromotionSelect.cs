using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KvjChess
{
    public partial class PromotionSelect : Form
    {
        private PieceType selectedType;

        internal PromotionSelect()
        {
            InitializeComponent();
            selectedType = PieceType.PAWN;
        }

        internal PieceType Prompt(IWin32Window owner)
        {
            ShowDialog(owner);
            return selectedType;
        }

        private void btnQueen_Click(object sender, EventArgs e)
        {
            selectedType = PieceType.QUEEN;
            this.Close();
        }

        private void btnKnight_Click(object sender, EventArgs e)
        {
            selectedType = PieceType.KNIGHT;
            this.Close();
        }

        private void btnRook_Click(object sender, EventArgs e)
        {
            selectedType = PieceType.ROOK;
            this.Close();
        }

        private void btnBishop_Click(object sender, EventArgs e)
        {
            selectedType = PieceType.BISHOP;
            this.Close();
        }
    }
}
