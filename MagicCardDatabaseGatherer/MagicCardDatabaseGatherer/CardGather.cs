﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BlackLotus.Cards;
using EixoX;
using Helpers;

namespace WindowsFormsApplication1
{
    public class CardGather
    {
        private string _startOfCardTable = "class=\"cardDetails\"";
        private CardReadViewee _viewee;

        public CardGather(CardReadViewee viewee)
        {
            this._viewee = viewee;
        }

        public bool ReadCard(int cardNumber)
        {
            try
            {
                Card card = new Card();
                string page = "";
                using (WebClient client = new WebClient())
                {
                    page = client.DownloadString("http://gatherer.wizards.com/pages/Card/Details.aspx?multiverseid=" + cardNumber);
                }

                if (!page.Contains(this._startOfCardTable))
                    return false;

                while(page.CountOcurrences(this._startOfCardTable) > 0)
                {
                    page = page.Substring(page.RightIndexOf(this._startOfCardTable));
                    string cardTable = page.Substring(0, page.IndexOf("</table>"));
                    
                    //  Gets Card Image
                    if (cardTable.Contains("ctl00_ctl00_ctl00_MainContent_SubContent_SubContent_cardImage"))
                    {
                        string image = cardTable.Substring(cardTable.RightIndexOf("multiverseid="));
                        card.Art = image.Substring(0, image.IndexOf("&amp;"));
                    }

                    //  Gets Card Name
                    if (cardTable.Contains("ctl00_ctl00_ctl00_MainContent_SubContent_SubContent_nameRow"))
                    {
                        cardTable = cardTable.Substring(cardTable.RightIndexOf("ctl00_ctl00_ctl00_MainContent_SubContent_SubContent_nameRow"));
                        string name = cardTable.Substring(cardTable.RightIndexOf("value\">"));
                        card.Name = name.Substring(0, name.IndexOf("</div>")).Trim();
                    }

                    //  Gets Card Mana Cost
                    if (cardTable.Contains("ctl00_ctl00_ctl00_MainContent_SubContent_SubContent_manaRow"))
                    {
                        cardTable = cardTable.Substring(cardTable.RightIndexOf("ctl00_ctl00_ctl00_MainContent_SubContent_SubContent_manaRow"));
                        string manaCost = cardTable.Substring(cardTable.RightIndexOf("value\">"));
                        manaCost = manaCost.Substring(0, manaCost.RightIndexOf("div>"));
                        while (manaCost.CountOcurrences("name=") > 0)
                        {
                            manaCost = manaCost.Substring(manaCost.RightIndexOf("name="));
                            card.ManaCost += manaCost.First();
                        }
                        card.ManaCost.Trim();
                    }

                    //  Gets Card Converted Mana Cost
                    if (cardTable.Contains("ctl00_ctl00_ctl00_MainContent_SubContent_SubContent_cmcRow"))
                    {
                        cardTable = cardTable.Substring(cardTable.RightIndexOf("ctl00_ctl00_ctl00_MainContent_SubContent_SubContent_cmcRow"));
                        string cmc = cardTable.Substring(cardTable.RightIndexOf("value\">"));
                        cmc = cmc.Substring(0, cmc.IndexOf("<")).Trim();
                        card.ConvertedManaCost = Int32.Parse(String.IsNullOrEmpty(cmc) ? "0" : cmc);
                    }

                    //  Gets Card Types
                    if (cardTable.Contains("ctl00_ctl00_ctl00_MainContent_SubContent_SubContent_typeRow"))
                    {
                        cardTable = cardTable.Substring(cardTable.RightIndexOf("ctl00_ctl00_ctl00_MainContent_SubContent_SubContent_typeRow"));
                        cardTable = cardTable.Substring(cardTable.RightIndexOf("value\">"));
                        string[] types = cardTable.Substring(0, cardTable.IndexOf("<")).Split(new string[]{"â€”"}, StringSplitOptions.RemoveEmptyEntries);
                        card.Type = types[0].Trim();
                        if (types.Length > 1 && !String.IsNullOrEmpty(types[1].Trim()))
                        {
                            card.SubType = types[1].Trim();
                        }
                    }

                    _viewee.OnCardRead(card);
                }
                return true;

            }
            catch (Exception ex)
            {
                _viewee.OnException(ex);
                return false;
            }
        }
    }
}
