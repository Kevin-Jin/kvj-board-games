using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KvjBoardGames.OnlineFunctions
{
    public static class CommonOpHeaders
    {
        public const byte
            END_SESSION = 0x00,
            FORFEIT = 0x01,
            RESET_GAME = 0x02,
            DRAW_REQUEST = 0x03,
            DRAW_RESPONSE = 0x04
        ;
    }

    public static class CommonPacketWriter
    {
        public static byte[] WriteForfeit()
        {
            return new byte[] { CommonOpHeaders.FORFEIT };
        }

        public static byte[] WriteResetGame()
        {
            return new byte[] { CommonOpHeaders.RESET_GAME };
        }

        public static byte[] WriteDrawRequest()
        {
            return new byte[] { CommonOpHeaders.DRAW_REQUEST };
        }

        public static byte[] WriteDrawResponse(bool agree)
        {
            return new byte[] { CommonOpHeaders.DRAW_RESPONSE, (byte) (agree ? 1 : 0) };
        }
    }
}
