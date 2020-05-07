using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;
using System.IO;

namespace CardCastToImage {
    public class CardcastCardDeck {
        public class CardcastCard {
            public string id { get; set; }
            public string[] text { get; set; }
            public string created_at { get; set; }
            public bool nsfw { get; set; }
            public string underlineSize = "______";

            public CardcastCard() {

            }

            public string finalCardString() {
                string retVal = "";
                if (text == null) {
                    return retVal;
                }
                for (int i = 0; i < text.Length - 1; i++) {
                    retVal += text[i] + underlineSize;
                }
                retVal += text[text.Length - 1];
                return retVal;
            }
        }

        public List<CardcastCard> calls { get; set; }
        public List<CardcastCard> responses { get; set; }

        public CardcastCardDeck() {

        }
    }

    class Program {
        static string BASE_URL = "https://api.cardcastgame.com/v1/decks/";
        static string BASE_END_URL = "/cards";
        static int cardWidth = 400;
        static int cardHeight = 560;   //2.5" x 3.5" = 1.4 aspect ratio

        static void RenderCallCardsSet(string deckID, CardcastCardDeck cardDeck, int offset, int setCount) {
            //Render call cards (black background, white text)
            int leftMargin = (int)(cardWidth * 0.1f);
            int rightMargin = (int)(cardWidth * 0.1f);
            int topMargin = (int)(cardHeight * 0.1f);
            int bottomMargin = (int)(cardHeight * 0.1f);

            Bitmap bmp = new Bitmap(cardWidth * 10, cardHeight * 7);
            Graphics g = Graphics.FromImage(bmp);
            Font f = new Font("Arial", 24, FontStyle.Bold);
            Font f2 = new Font("Arial", 12, FontStyle.Bold);
            Brush b = new SolidBrush(Color.White);
            Pen p = new Pen(b);
            p.Width = 1;

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.Clear(Color.FromArgb(255, 0, 0, 0));

            //CardcastCardDeck.CardcastCard temp = new CardcastCardDeck.CardcastCard();
            //temp.text = new string[] { "ThisIsAReallyLongStringToMakeSureNothingBadHappens 2nd line ", " <--line--> ", " OwO." };
            //cardDeck.calls.Add(temp);

            int cardIndex = offset;
            int cardCounter = 0;
            for (int cardRow = 0; cardRow < 7; cardRow++) {   //Row
                bool finishedCards = false;
                for (int cardColumn = 0; cardColumn < 10; cardColumn++) {  //Column
                    if (cardIndex >= cardDeck.calls.Count) {
                        finishedCards = true;
                        break;
                    }

                    g.SetClip(new Rectangle(leftMargin, topMargin, cardWidth - rightMargin - leftMargin, cardHeight - topMargin - bottomMargin));

                    string[] cardString = cardDeck.calls[cardIndex].finalCardString().Split(new char[] { ' ' });
                    List<string> finalCardStrings = new List<string>();

                    string strLine = "";
                    SizeF cardTextSize;
                    bool atLeastOneWordFits = false;
                    for (int word = 0; word < cardString.Length; word++) {
                        cardTextSize = g.MeasureString(strLine + cardString[word], f);
                        if (cardTextSize.Width > cardWidth - leftMargin - rightMargin) {
                            //Too long of a word to fit on one line, ellipse
                            if (atLeastOneWordFits == false) {
                                finalCardStrings.Add(cardString[word]);
                            } else {
                                finalCardStrings.Add(strLine);
                                word--;
                            }
                            strLine = "";
                            atLeastOneWordFits = false;
                        } else {
                            //current string still fits, keep appending words
                            strLine += cardString[word] + " ";
                            atLeastOneWordFits = true;
                        }
                    }
                    if (atLeastOneWordFits == true) {
                        finalCardStrings.Add(strLine);
                    }

                    for (int cardLine = 0; cardLine < finalCardStrings.Count; cardLine++) {
                        cardTextSize = g.MeasureString(finalCardStrings[cardLine], f);
                        g.DrawString(finalCardStrings[cardLine], f, b, leftMargin, cardLine * cardTextSize.Height + topMargin);
                    }

                    cardTextSize = g.MeasureString(deckID, f2);
                    g.DrawString(deckID, f2, b, cardWidth - rightMargin - cardTextSize.Width, cardHeight - bottomMargin - cardTextSize.Height);

                    g.TranslateTransform(cardWidth, 0);
                    cardIndex++;
                    cardCounter++;
                }

                if (finishedCards == true) {
                    break;
                }

                g.ResetTransform();
                g.TranslateTransform(0, (cardRow + 1) * cardHeight);
            }

            bmp.Save(deckID + "-BlackCardsFrontFace-" + cardCounter.ToString() + "-" + (setCount + 1).ToString() + ".png");
            g.Dispose();
            bmp.Dispose();
        }

        static Bitmap RenderCallCardBackFace() {
            int leftMargin = (int)(cardWidth * 0.1f);
            int rightMargin = (int)(cardWidth * 0.1f);
            int topMargin = (int)(cardHeight * 0.1f);
            int bottomMargin = (int)(cardHeight * 0.1f);

            Bitmap bmp = new Bitmap(cardWidth, cardHeight);
            Graphics g = Graphics.FromImage(bmp);
            Font f = new Font("Arial", 36, FontStyle.Bold);
            Brush b = new SolidBrush(Color.White);
            Pen p = new Pen(b);
            p.Width = 1;

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.Clear(Color.FromArgb(255, 0, 0, 0));

            SizeF textSize;
            string s;
            int lineY = topMargin;
            s = "TOTALLY";
            textSize = g.MeasureString(s, f);
            g.DrawString(s, f, b, leftMargin, lineY);
            lineY += (int)textSize.Height;
            s = "NOT";
            textSize = g.MeasureString(s, f);
            g.DrawString(s, f, b, leftMargin, lineY);
            lineY += (int)textSize.Height;
            s = "CARDS";
            textSize = g.MeasureString(s, f);
            g.DrawString(s, f, b, leftMargin, lineY);
            lineY += (int)textSize.Height;
            s = "AGAINST";
            textSize = g.MeasureString(s, f);
            g.DrawString(s, f, b, leftMargin, lineY);
            lineY += (int)textSize.Height;
            s = "HUMANITY";
            textSize = g.MeasureString(s, f);
            g.DrawString(s, f, b, leftMargin, lineY);
            lineY += (int)textSize.Height;

            f = new Font("Arial", 12, FontStyle.Bold);
            s = "AB";
            textSize = g.MeasureString(s, f);
            g.DrawString(s, f, b, cardWidth - rightMargin - textSize.Width, cardHeight - bottomMargin - textSize.Height);

            g.Dispose();

            return bmp;
        }

        static void RenderResponseCardsSet(string deckID, CardcastCardDeck cardDeck, int offset, int setCount) {
            //Render call cards (black background, white text)
            int leftMargin = (int)(cardWidth * 0.1f);
            int rightMargin = (int)(cardWidth * 0.1f);
            int topMargin = (int)(cardHeight * 0.1f);
            int bottomMargin = (int)(cardHeight * 0.1f);

            Bitmap bmp = new Bitmap(cardWidth * 10, cardHeight * 7);
            Graphics g = Graphics.FromImage(bmp);
            Font f = new Font("Arial", 24, FontStyle.Bold);
            Font f2 = new Font("Arial", 12, FontStyle.Bold);
            Brush b = new SolidBrush(Color.Black);
            Pen p = new Pen(b);
            p.Width = 1;

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.Clear(Color.FromArgb(255, 255, 255, 255));

            //CardcastCardDeck.CardcastCard temp = new CardcastCardDeck.CardcastCard();
            //temp.text = new string[] { "ThisIsAReallyLongStringToMakeSureNothingBadHappens 2nd line ", " <--line--> ", " OwO." };
            //cardDeck.calls.Add(temp);

            int cardIndex = offset;
            int cardCounter = 0;
            for (int cardRow = 0; cardRow < 7; cardRow++) {   //Row
                bool finishedCards = false;
                for (int cardColumn = 0; cardColumn < 10; cardColumn++) {  //Column
                    if (cardIndex >= cardDeck.responses.Count) {
                        finishedCards = true;
                        break;
                    }

                    g.SetClip(new Rectangle(leftMargin, topMargin, cardWidth - rightMargin - leftMargin, cardHeight - topMargin - bottomMargin));

                    string[] cardString = cardDeck.responses[cardIndex].finalCardString().Split(new char[] { ' ' });
                    List<string> finalCardStrings = new List<string>();

                    string strLine = "";
                    SizeF cardTextSize;
                    bool atLeastOneWordFits = false;
                    for (int word = 0; word < cardString.Length; word++) {
                        cardTextSize = g.MeasureString(strLine + cardString[word], f);
                        if (cardTextSize.Width > cardWidth - leftMargin - rightMargin) {
                            //Too long of a word to fit on one line, ellipse
                            if (atLeastOneWordFits == false) {
                                finalCardStrings.Add(cardString[word]);
                            } else {
                                finalCardStrings.Add(strLine);
                                word--;
                            }
                            strLine = "";
                            atLeastOneWordFits = false;
                        } else {
                            //current string still fits, keep appending words
                            strLine += cardString[word] + " ";
                            atLeastOneWordFits = true;
                        }
                    }
                    if (atLeastOneWordFits == true) {
                        finalCardStrings.Add(strLine);
                    }

                    for (int cardLine = 0; cardLine < finalCardStrings.Count; cardLine++) {
                        cardTextSize = g.MeasureString(finalCardStrings[cardLine], f);
                        g.DrawString(finalCardStrings[cardLine], f, b, leftMargin, cardLine * cardTextSize.Height + topMargin);
                    }

                    cardTextSize = g.MeasureString(deckID, f2);
                    g.DrawString(deckID, f2, b, cardWidth - rightMargin - cardTextSize.Width, cardHeight - bottomMargin - cardTextSize.Height);

                    g.TranslateTransform(cardWidth, 0);
                    cardIndex++;
                    cardCounter++;
                }

                if (finishedCards == true) {
                    break;
                }

                g.ResetTransform();
                g.TranslateTransform(0, (cardRow + 1) * cardHeight);
            }

            bmp.Save(deckID + "-WhiteCardsFrontFace-" + cardCounter.ToString() + "-" + (setCount + 1).ToString() + ".png");

            g.Dispose();
            bmp.Dispose();
        }

        static Bitmap RenderResponseCardBackFace() {
            int leftMargin = (int)(cardWidth * 0.1f);
            int rightMargin = (int)(cardWidth * 0.1f);
            int topMargin = (int)(cardHeight * 0.1f);
            int bottomMargin = (int)(cardHeight * 0.1f);

            Bitmap bmp = new Bitmap(cardWidth, cardHeight);
            Graphics g = Graphics.FromImage(bmp);
            Font f = new Font("Arial", 36, FontStyle.Bold);
            Brush b = new SolidBrush(Color.Black);
            Pen p = new Pen(b);
            p.Width = 1;

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.Clear(Color.FromArgb(255, 255, 255, 255));

            SizeF textSize;
            string s;
            int lineY = topMargin;
            s = "TOTALLY";
            textSize = g.MeasureString(s, f);
            g.DrawString(s, f, b, leftMargin, lineY);
            lineY += (int)textSize.Height;
            s = "NOT";
            textSize = g.MeasureString(s, f);
            g.DrawString(s, f, b, leftMargin, lineY);
            lineY += (int)textSize.Height;
            s = "CARDS";
            textSize = g.MeasureString(s, f);
            g.DrawString(s, f, b, leftMargin, lineY);
            lineY += (int)textSize.Height;
            s = "AGAINST";
            textSize = g.MeasureString(s, f);
            g.DrawString(s, f, b, leftMargin, lineY);
            lineY += (int)textSize.Height;
            s = "HUMANITY";
            textSize = g.MeasureString(s, f);
            g.DrawString(s, f, b, leftMargin, lineY);
            lineY += (int)textSize.Height;

            f = new Font("Arial", 12, FontStyle.Bold);
            s = "AB";
            textSize = g.MeasureString(s, f);
            g.DrawString(s, f, b, cardWidth - rightMargin - textSize.Width, cardHeight - bottomMargin - textSize.Height);

            g.Dispose();

            return bmp;
        }

        static void Main(string[] args) {
            string[] cardDecksArg = null;
            if (args.Length == 1) {
                cardDecksArg = args[0].Split(new char[] { ',' });
            }

            if (cardDecksArg == null) {
                return;
            }

            foreach (string s in cardDecksArg) {
                //HTTPS GET
                Console.Write("Attempting to fetch card deck: " + s + "...");
                HttpClient client = new HttpClient();
                HttpResponseMessage response = client.GetAsync(BASE_URL + s + BASE_END_URL).Result;
                if (response.IsSuccessStatusCode) {
                    Console.WriteLine("OK");
                } else {
                    Console.WriteLine("ERROR");
                    continue;
                }

                //Parse JSON to object
                Console.Write("Attempting to parse result...");
                CardcastCardDeck cardDeck = null;
                try {
                    string responseString = response.Content.ReadAsStringAsync().Result;
                    cardDeck = JsonConvert.DeserializeObject<CardcastCardDeck>(responseString);
                    Console.WriteLine("OK (" + cardDeck.calls.Count.ToString() + ", " + cardDeck.responses.Count.ToString() + ")");
                } catch (Exception ex) {
                    Console.WriteLine("ERROR: " + ex.Message);
                    continue;
                }

                int callsCardSetCount = 0;
                for (int i = 0; i < cardDeck.calls.Count; i += 10 * 7) {
                    RenderCallCardsSet(s, cardDeck, i, callsCardSetCount);
                    callsCardSetCount++;
                }
                RenderCallCardBackFace().Save(s + "-BlackCardsBackFace.png");

                int responseCardSetCount = 0;
                for (int i = 0; i < cardDeck.responses.Count; i += 10 * 7) {
                    RenderResponseCardsSet(s, cardDeck, i, responseCardSetCount);
                    responseCardSetCount++;
                }
                RenderResponseCardBackFace().Save(s + "-WhiteCardsBackFace.png");

                //Don't want to piss of the server timeouts
                System.Threading.Thread.Sleep(1000);
            }

            Console.ReadLine();
        }
    }
}
