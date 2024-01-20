using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace TextProcessor.Classes
{
    [Flags]
    public enum FindOptions
    {
        None = 0x00000000,
        MatchCase = 0x00000001,
        MatchWholeWord = 0x00000002,
    }
    class FindAndReplaceManager
    {
        private FlowDocument inputDocument;
        private TextPointer currentPosition;

        public FindAndReplaceManager(FlowDocument inputDocument)
        {
            if (inputDocument == null)
            {
                throw new ArgumentNullException("documentToFind");
            }

            this.inputDocument = inputDocument;
            this.currentPosition = inputDocument.ContentStart;
        }

        public TextPointer CurrentPosition
        {
            get
            {
                return currentPosition;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.CompareTo(inputDocument.ContentStart) < 0 ||
                    value.CompareTo(inputDocument.ContentEnd) > 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                currentPosition = value;
            }
        }

        public TextRange FindNext(String input, FindOptions findOptions)
        {
            TextRange textRange = GetTextRangeFromPosition(ref currentPosition, input, findOptions);
            return textRange;
        }

        public TextRange Replace(String input, String replacement, FindOptions findOptions)
        {
            TextRange textRange = FindNext(input, findOptions);
            if (textRange != null)
            {
                textRange.Text = replacement;
            }

            return textRange;
        }

        public Int32 ReplaceAll(String input, String replacement, FindOptions findOptions, Action<TextRange> action)
        {
            Int32 count = 0;
            currentPosition = inputDocument.ContentStart;
            while (currentPosition.CompareTo(inputDocument.ContentEnd) < 0)
            {
                TextRange textRange = Replace(input, replacement, findOptions);
                if (textRange != null)
                {
                    count++;
                    if (action != null)
                    {
                        action(textRange);
                    }
                }
            }

            return count;
        }

        public TextRange GetTextRangeFromPosition(ref TextPointer position,
                                                  String input,
                                                  FindOptions findOptions)
        {
            Boolean matchCase = (findOptions & FindOptions.MatchCase) == FindOptions.MatchCase;
            Boolean matchWholeWord = (findOptions & FindOptions.MatchWholeWord)
                                                        == FindOptions.MatchWholeWord;

            TextRange textRange = null;

            while (position != null)
            {
                if (position.CompareTo(inputDocument.ContentEnd) == 0)
                {
                    break;
                }

                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    String textRun = position.GetTextInRun(LogicalDirection.Forward);
                    StringComparison stringComparison = matchCase ?
                        StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
                    Int32 indexInRun = textRun.IndexOf(input, stringComparison);

                    if (indexInRun >= 0)
                    {
                        position = position.GetPositionAtOffset(indexInRun);
                        TextPointer nextPointer = position.GetPositionAtOffset(input.Length);
                        textRange = new TextRange(position, nextPointer);

                        if (matchWholeWord)
                        {
                            if (IsWholeWord(textRange))
                            {
                                position = position.GetPositionAtOffset(input.Length);
                                break;
                            }
                            else
                            {
                                position = position.GetPositionAtOffset(input.Length);
                                return GetTextRangeFromPosition(ref position, input, findOptions);
                            }
                        }
                        else
                        {
                            position = position.GetPositionAtOffset(input.Length);
                            break;
                        }
                    }
                    else
                    {
                        position = position.GetPositionAtOffset(textRun.Length);
                    }
                }
                else
                {
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                }
            }

            return textRange;
        }

        private Boolean IsWordChar(Char character)
        {
            return Char.IsLetterOrDigit(character) || character == '_';
        }

        private Boolean IsWholeWord(TextRange textRange)
        {
            Char[] chars = new Char[1];

            if (textRange.Start.CompareTo(inputDocument.ContentStart) == 0 || textRange.Start.IsAtLineStartPosition)
            {
                textRange.End.GetTextInRun(LogicalDirection.Forward, chars, 0, 1);
                return !IsWordChar(chars[0]);
            }
            else if (textRange.End.CompareTo(inputDocument.ContentEnd) == 0)
            {
                textRange.Start.GetTextInRun(LogicalDirection.Backward, chars, 0, 1);
                return !IsWordChar(chars[0]);
            }
            else
            {
                textRange.End.GetTextInRun(LogicalDirection.Forward, chars, 0, 1);
                if (!IsWordChar(chars[0]))
                {
                    textRange.Start.GetTextInRun(LogicalDirection.Backward, chars, 0, 1);
                    return !IsWordChar(chars[0]);
                }
            }

            return false;
        }
    }
}
