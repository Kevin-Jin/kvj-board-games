using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace KvjBoardGames
{
    internal static class CommonFunctions
    {
        internal static bool OpenSaveFile(GameBoard board)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "Kvj Save File (*.kvj)|*.kvj";
            if (d.ShowDialog(board) != DialogResult.Cancel)
            {
                try
                {
                    using (FileStream fs = (System.IO.FileStream)d.OpenFile())
                    {
                        byte[] bytes = new byte[fs.Length];
                        fs.Read(bytes, 0, bytes.Length);
                        board.Deserialize(bytes);
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
