using System;

namespace GameJSON.ManualParsing.Utils
{
    public static class InPlaceParsing
    {
        public static double ParseDouble(string toParse, int start, int length)
        {
            bool negative = false;
            if (toParse[start] == '-')
            {
                negative = true;
                start++;
                length--;
            }

            int exponent = 0;
            {
                for (int i = start; i < start + length; i++)
                {
                    if (toParse[i] == 'e' || toParse[i] == 'E')
                    {
                        int eIndex = i;
                        bool expNegative = false;
                        if (toParse[i + 1] == '+')
                        {
                            i++;
                        }
                        else if (toParse[i + 1] == '-')
                        {
                            expNegative = true;
                            i++;
                        }

                        int tensSlot = 1;
                        for (int exponentIndex = start + length - 1; exponentIndex > i; exponentIndex--)
                        {
                            int val = toParse[exponentIndex] - '0';
                            exponent += val * tensSlot;
                            tensSlot *= 10;
                        }

                        if (expNegative)
                        {
                            exponent *= -1;
                        }

                        length = eIndex - start;
                        break;
                    }
                }
            }

            if (toParse[start] < '0' || toParse[start] > '9')
            {
                throw new Exception($"Expected first character to be either a - or 1 to 9 and got {toParse[start]}");
            }

            int decimalPointPos = start + length;
            {
                for (int i = start; i < start + length; i++)
                {
                    if (toParse[i] == '.')
                    {
                        decimalPointPos = i;
                        break;
                    }
                }
            }

            double value = 0;

            // Calculate values above decimal place
            {
                double tensSlot = 1;
                for (int i = decimalPointPos - 1; i >= start; i--)
                {
                    double slotVal = (double)(toParse[i] - '0');
                    value += tensSlot * slotVal;
                    tensSlot *= 10d;
                }
            }

            // Calculate values below decimal place
            {
                double tensSlot = 0.1d;
                for (int i = decimalPointPos + 1; i < start + length; i++)
                {
                    double slotVal = (double)(toParse[i] - '0');
                    value += tensSlot * slotVal;
                    tensSlot /= 10d;
                }
            }

            value *= Math.Pow(10, exponent);
            if (negative)
            {
                value *= -1;
            }
            return value;
        }
    }
}
