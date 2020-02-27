using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Platfarm.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Components
{
    public class DialogueNode
    {
        public string dialogueText;
        public string decisionUpText, decisionDownText, decisionLeftText, decisionRightText;
        public DialogueNode decisionUpNode, decisionDownNode, decisionLeftNode, decisionRightNode;
        public Texture2D portrait;
        public Action<EntityPlayer> onActivation;
        public bool activated;

        public static Texture2D PORTRAIT_BAD;

        public static void LoadPortraits()
        {
            PORTRAIT_BAD = Plateau.CONTENT.Load<Texture2D>("interface/portrait_test");
        }

        public DialogueNode(string dialogueText, Texture2D portrait, Action<EntityPlayer> onActivation = null, 
            string decisionUpText = "", DialogueNode decisionUp = null, 
            string decisionLeftText = "", DialogueNode decisionLeft = null,
            string decisionRightText = "", DialogueNode decisionRight = null,
            string decisionDownText = "", DialogueNode decisionDown = null)
        {
            this.activated = false;
            this.dialogueText = dialogueText;
            this.decisionUpText = decisionUpText;
            this.decisionRightText = decisionRightText;
            this.decisionLeftText = decisionLeftText;
            this.decisionDownText = decisionDownText;
            this.decisionUpNode = decisionUp;
            this.decisionDownNode = decisionDown;
            this.decisionLeftNode = decisionLeft;
            this.decisionRightNode = decisionRight;
            this.portrait = portrait;
            this.onActivation = onActivation;
        }

        public bool IsFinished()
        {
            return decisionUpNode == null;
        }

        public void SetNext(DialogueNode next)
        {
            decisionUpNode = next;
        }

        public DialogueNode GetNext()
        {
            return decisionUpNode;
        }

        public bool Splits()
        {
            return decisionDownNode != null || decisionRightNode != null || decisionLeftNode != null;
        }

        public void OnActivation(EntityPlayer player)
        {
            if (!activated && onActivation != null)
            {
                this.onActivation(player);
            }
        }

        public void DrawText(SpriteBatch sb, Vector2 location, int numChars)
        {
            string toDraw = "";
            if (numChars > dialogueText.Length)
            {
                toDraw = dialogueText;
            } else {
                toDraw = dialogueText.Substring(0, numChars);
            }

            sb.DrawString(Plateau.FONT, toDraw, location, Color.Black, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
        }

    }
}
