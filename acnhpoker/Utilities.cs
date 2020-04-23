﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace acnhpoker
{
    class Utilities
    {
        UInt32 ItemSlotBase = 0xAC4723D0;
        UInt32 ItemSlot21Base = 0xAC472318;
        public Utilities()
        {
        }

        private static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public string GetItemSlotAddress(int slot)
        {
            if (slot <= 20)
            {
                return "0x" + (ItemSlotBase + ((Clamp(slot, 1, 20) - 1) * 0x8)).ToString("X");
            }
            else
            {
                return "0x" + (ItemSlot21Base + ((Clamp(slot, 21, 40) - 21) * 0x8)).ToString("X");
            }
        }

        public string GetItemCountAddress(int slot)
        {
            if (slot <= 20)
            {
                return "0x" + (ItemSlotBase + ((Clamp(slot, 1, 20) - 1) * 0x8) + 0x4).ToString("X");
            }
            else
            {
                return "0x" + (ItemSlot21Base + ((Clamp(slot, 21, 40) - 21) * 0x8) + 0x4).ToString("X");
            }
        }

        public byte[] GetInventory(Socket socket)
        {
            try
            {
                byte[] msg = Encoding.UTF8.GetBytes("peek " + GetItemSlotAddress(1) + " 1024\r\n");
                Debug.Print("peek " + GetItemSlotAddress(1) + " 1024\r\n");
                socket.Send(msg);

                byte[] b = new byte[1024];
                socket.Receive(b);
                return b;
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
                return null;
            }
        }
        public byte[] GetInventoryBank1(Socket socket)
        {
            try
            {
                byte[] msg = Encoding.UTF8.GetBytes("peek " + GetItemSlotAddress(1) + " 314\r\n");
                Debug.Print("peek " + GetItemSlotAddress(1) + " 314\r\n");
                socket.Send(msg);

                byte[] b = new byte[1024];
                socket.Receive(b);

                return b;
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
                return null;
            }
        }

        // Gets slots 21-40 (Inventory bank 2)
        public byte[] GetInventoryBank2(Socket socket)
        {

            try
            {
                byte[] msg = Encoding.UTF8.GetBytes("peek " + GetItemSlotAddress(21) + " 314\r\n");
                Debug.Print("peek " + GetItemSlotAddress(21) + " 314\r\n");

                int first = socket.Send(msg);
                while (!socket.Poll(-1, SelectMode.SelectRead))
                {
                    System.Threading.Thread.Sleep(5);
                }
                byte[] b = new byte[1024];
                int first_rec = socket.Receive(b);


                return b;
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
                return null;
            }
        }
        public bool SpawnItem(Socket socket, int slot, String value, int amount, string categ)
        {
            try
            {

                byte[] msg = Encoding.UTF8.GetBytes("poke " + GetItemSlotAddress(slot) + " " + FormatItemId(value) + "\r\n");
                //Debug.Print(Encoding.ASCII.GetString(msg));

                socket.Send(msg);

                if (amount != 1 & string.Equals(categ, "Recipes"))
                {
                    var itemCount = GetItemCountAddress(slot);

                    byte[] countMsg = Encoding.UTF8.GetBytes("poke " + itemCount + " " + FormatItemId((amount).ToString("X")) + "\r\n");
                    socket.Send(countMsg);
                }
                else
                {
                    var itemCount = GetItemCountAddress(slot);

                    byte[] countMsg = Encoding.UTF8.GetBytes("poke " + itemCount + " " + (amount - 1).ToString("X") + "\r\n");
                    socket.Send(countMsg);
                }
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }

            return false;
        }

        public string FormatItemId(String itemId)
        {
            string n0 = String.Concat(Enumerable.Repeat("0", 4 - itemId.Length));
            string preFlip = String.Concat(n0, itemId);
            string firstHalf = preFlip.Substring(0, 2);
            string secondHalf = preFlip.Substring(2, 2);
            string postFlip = "0x" + secondHalf + firstHalf;
            //probaby a better way to do this lol

            return postFlip;
        }

        public string UnflipItemId(String itemId)
        {
            string firstHalf = itemId.Substring(0, 2).TrimStart(new char[] { '0' });
            string secondHalf = itemId.Substring(2, 2).TrimStart(new char[] { '0' });
            string postFlip = secondHalf + firstHalf;
            return postFlip;
        }

    }
}
