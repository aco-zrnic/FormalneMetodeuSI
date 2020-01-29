using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;

namespace IzmjenaPrograma
{
    struct Variable
    {
        public string nameOfVar;
        public string typeOfVar; //Number od String
        public string strings;
        public Variable(string str1 = "", string str2 = "")
        {
            strings = "";
            nameOfVar = str1;
            typeOfVar = str2;
        }
    }

    class TextValid
    {
        public int checkForInvalidChar(string str)
        {
            int decissonNumb = 0;
            Match m = Regex.Match(str, @"[a-zA-Z0-9\.\$\#\<\>\+\-\*\/\%\=\(\)\[\]\@\s\t\" + "\u0022" + @"\{\}\@_]+");
            if (!str.Equals(m.ToString()))
            {
                Console.WriteLine("Contains unregular characters!->{0}", str);
                decissonNumb = 1;

            }
            return decissonNumb;
        }
        public int BraceCheck(string str)
        {
            int decissonNumb, countBrace;
            decissonNumb = countBrace = 0;
            foreach (char character in str)
            {
                if (character == '[') countBrace++;
                if (character == ']') countBrace--;
                if (countBrace >= 2 || countBrace <= -1)
                {
                    Console.WriteLine("Greska u prevodu zagrada");
                    decissonNumb = 1;
                    return decissonNumb;
                }
            }
            return decissonNumb;
        }
        public int AllCapsCheck(string str)
        {
            int counterAllCaps, decissonNumb;
            decissonNumb = counterAllCaps = 0;
            MatchCollection matches = Regex.Matches(str, @"\<[^>]*\>");
            foreach (Match m in matches)
            {
                if (m.ToString() == "<all_caps>") counterAllCaps++;
                else if (m.ToString() == "</all_caps>") counterAllCaps--;
                else
                {
                    Console.WriteLine("Invalid use of <>");
                    decissonNumb = 1;
                    return decissonNumb;
                }
            }
            if (counterAllCaps != 0)
            {
                Console.WriteLine("Invalid use of <all_caps>");
            }
            return decissonNumb;
        }
        public string ToUpper(string str)
        {
            MatchCollection matches = Regex.Matches(str, @"\<all_caps\>.+\<\/all_caps\>");
            foreach (Match m in matches)
            {
                string pom_str = m.ToString();
                pom_str = pom_str.Replace("<all_caps>", "");
                pom_str = pom_str.Replace("</all_caps>", "");
                pom_str = pom_str.ToUpper();
                str = Regex.Replace(str, @"\<all_caps\>.+\<\/all_caps\>", pom_str);
            }
            return str;
        }
        public double Evaluate(string expression)
        {
            var loDataTable = new DataTable();
            var loDataColumn = new DataColumn("Eval", typeof(double), expression);
            loDataTable.Columns.Add(loDataColumn);
            loDataTable.Rows.Add(0);
            return (double)(loDataTable.Rows[0]["Eval"]);
        }
        public bool DoesThisVarExist(Variable varOb, ArrayList arrayList)
        {
            for (int i = 0; i < arrayList.Count; i++)
            {
                Variable pom = (Variable)arrayList[i];
                if (pom.nameOfVar == varOb.nameOfVar && pom.typeOfVar != varOb.typeOfVar)
                {
                    return true;
                }
                else if (pom.nameOfVar == varOb.nameOfVar)
                {
                    arrayList.RemoveAt(i);
                    arrayList.Add(varOb);
                    return false;
                }
            }
            arrayList.Add(varOb);//VEOMA BITNO!,jer nulti(odnosno prvi) element se odmah dodaje
            return false;
        }
        public bool ArithmeticVarCheck(string str, MatchCollection matchfir, ArrayList arrayList)
        {
            int counterOfPlusAndSpace = 0;
            int counterOfPlus = 0; //broji jel svaki drugi tip aritmeticki odnosno +,jer to mora da bude tako jer bi inace bilo string[var] prihvatljivo
            Variable varOb = new Variable();

            foreach (Match m in matchfir)
            {
                foreach (Variable var in arrayList)
                {
                    if (var.nameOfVar == m.Groups[3].Value && var.typeOfVar == "String")
                    {
                        return ComplexVarCheck(str, matchfir, arrayList);
                    }
                }
            }

            for (int i = 0; i < matchfir.Count; i++)  //foreach (Match m in matchsec)
            {
                if (matchfir[i].Groups[1].Value != "" && i != 0) //ukoliko se pojavi [var]= negdje gdje nije pocetak
                {
                    Console.WriteLine("Ne pravilna deklaracija varijable!->{0}", str);
                    return true;
                }
                else if (matchfir[0].Groups[1].Value != "")
                {
                    varOb.nameOfVar = matchfir[0].Groups[1].Value;
                    varOb.typeOfVar = "Number";

                    if ((i == 1 && matchfir[i].Groups[2].Value != "") || (i == matchfir.Count - 1 && matchfir[i].Groups[2].Value != ""))  //provjera jel prvi znak + pa onda string sledi ili zadnji znak +
                    {
                        foreach (char character in matchfir[i].Groups[2].Value)
                        {
                            if (character == '+' || character == ' ' || character == 9) //moze bez uslova za " " i 9,kasnije mozda izbacim,PROVJERITI JOS SLUCAJEVA!
                                counterOfPlusAndSpace++;
                        }
                        if (counterOfPlusAndSpace == matchfir[i].Groups[2].Length)
                        {
                            Console.WriteLine("Ne moze ici + prvo pa onda string ili sam + na kraju->{0}", str);
                            return true;
                        }
                        counterOfPlusAndSpace = 0;
                    }
                    if (Regex.IsMatch(matchfir[i].Value, @"\[([\w\d\.\$\#_]+)\]"))
                    {
                        if (matchfir[i].Groups[3].Value != "")
                        {
                            counterOfPlus++;
                            int decissonVar = 0; //ako postoji ta Var onda nece nist uradit,ali ako ne postoji onda baca error;
                            foreach (Variable var in arrayList)
                            {
                                if (var.nameOfVar.Equals(matchfir[i].Groups[3].Value)) //ako postoji promjenljiva
                                {
                                    varOb.strings += "+";
                                    varOb.strings += var.strings;
                                    varOb.strings += "+";
                                    decissonVar = 1;
                                    break;
                                }
                            }
                            if (decissonVar == 0)
                            {
                                Console.WriteLine("Ne postoji ta promjenljiva!->{0}", str);
                                return true;
                            }
                        }
                    }
                    else if (Regex.IsMatch(matchfir[i].Value, @"[\d\(\)\+\-\/\*\s]+"))
                    {
                        counterOfPlus--;
                        foreach (char character in matchfir[i].Groups[2].Value) //ako je znak samo + da onda doda iduci string u konkatenaciju
                        {
                            if (character == '+' || character == ' ' || character == 9) //moze bez uslova za " " i 9,kasnije mozda izbacim,PROVJERITI JOS SLUCAJEVA!
                                counterOfPlusAndSpace++;
                        }
                        if (counterOfPlusAndSpace != matchfir[i].Groups[2].Length)
                        {
                            MatchCollection msubfirstChar = Regex.Matches(matchfir[i].Groups[2].Value, @"\s*\+?\(*\d?");
                            if (!msubfirstChar[0].Value.Contains("+") && i > 1)
                            {
                                Console.WriteLine("Pogresno definisan aritmeticki izraz,fali PLUS na pocetku->{0}", str);
                                return true;
                            }
                            else
                            {
                                varOb.strings += "+";
                            }
                            try
                            {
                                varOb.strings += Evaluate(matchfir[i].Groups[2].Value + "+0").ToString();
                            }
                            catch (System.Data.SyntaxErrorException)
                            {
                                Console.WriteLine("Pogresno definisan aritmeticki izraz2 ->{0}", str);
                                return true;
                            }

                            MatchCollection msub = Regex.Matches(matchfir[i].Groups[2].Value, @"\)?\s*\+");
                            if (matchfir.Count > 2 && !msub[msub.Count - 1].Value.Contains("+"))
                            {
                                Console.WriteLine("Pogresno definisan aritmeticki izraz,fali PLUS na kraju->{0}", str);
                                return true;
                            }
                            else
                            {
                                varOb.strings += "+";
                            }
                        }

                        counterOfPlusAndSpace = 0;
                        if (counterOfPlus >= 2)
                        {
                            Console.WriteLine("Fali plus izmedju->{0}", str);
                            return true;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Mora poceti sa [var]->{0}", str);
                    return true;
                }
            }
            varOb.strings = Evaluate(varOb.strings + "+0").ToString();
            if (DoesThisVarExist(varOb, arrayList))
            {
                Console.WriteLine("Ne moze doci do promjene tipa variable->{0}", str);
                return true;
            }
            else
            {
                //Console.WriteLine(varOb.strings);
                return false;
            }
        }

        public bool ComplexVarCheck(string str, MatchCollection matchsec, ArrayList arrayList)
        {
            int counterOfPlusAndSpace = 0;
            int counterOfPlus = 0; //broji jel svaki drugi tip aritmeticki odnosno +,jer to mora da bude tako jer bi inace bilo string[var] prihvatljivo
            Variable varOb = new Variable();
            for (int i = 0; i < matchsec.Count; i++)  //foreach (Match m in matchsec)
            {

                if (matchsec[i].Groups[1].Value != "" && i != 0) //ukoliko se pojavi [var]= negdje gdje nije pocetak
                {
                    Console.WriteLine("Ne pravilna deklaracija varijable!->{0}", str);
                    return true;
                }
                else if (matchsec[0].Groups[1].Value != "")
                {
                    varOb.nameOfVar = matchsec[0].Groups[1].Value;
                    varOb.typeOfVar = "String";

                    if ((i == 1 && matchsec[i].Groups[3].Value != "") || (i == matchsec.Count - 1 && matchsec[i].Groups[3].Value != ""))  //provjera jel prvi znak + pa onda string sledi ili zadnji znak +
                    {
                        foreach (char character in matchsec[i].Groups[3].Value)
                        {
                            if (character == '+' || character == ' ' || character == 9) //moze bez uslova za " " i 9,kasnije mozda izbacim,PROVJERITI JOS SLUCAJEVA!
                                counterOfPlusAndSpace++;
                        }
                        if (counterOfPlusAndSpace == matchsec[i].Groups[3].Length)
                        {
                            Console.WriteLine("Ne moze ici + prvo pa onda string ili sam + na kraju->{0}", str);
                            return true;
                        }
                        counterOfPlusAndSpace = 0;
                    }

                    if (Regex.IsMatch(matchsec[i].Value, "\u0022" + @"[\w\d\s\.\$\#_]+" + "\u0022"))
                    {
                        counterOfPlus++;
                        varOb.strings += matchsec[i].Groups[2].Value;
                    }
                    else if (Regex.IsMatch(matchsec[i].Value, @"\[([\w\d\.\$\#_]+)\]"))
                    {
                        if (matchsec[i].Groups[4].Value != "")
                        {
                            counterOfPlus++;
                            int decissonVar = 0; //ako postoji ta Var onda nece nist uradit,ali ako ne postoji onda baca error;
                            foreach (Variable var in arrayList)
                            {
                                if (var.nameOfVar.Equals(matchsec[i].Groups[4].Value))
                                {
                                    varOb.strings += var.strings;
                                    decissonVar = 1;
                                    break;
                                }
                            }
                            if (decissonVar == 0)
                            {
                                Console.WriteLine("Ne postoji ta promjenljiva!->{0}", str);
                                return true;
                            }
                        }
                    }
                    else if (Regex.IsMatch(matchsec[i].Value, @"[\d\(\)\+\-\/\*\s]+"))
                    {
                        if (counterOfPlus > 0 && matchsec[i].Groups[3].Value.Contains("+"))
                            counterOfPlus--;
                        foreach (char character in matchsec[i].Groups[3].Value) //ako je znak samo + da onda doda iduci string u konkatenaciju
                        {
                            if (character == '+' || character == ' ' || character == 9) //moze bez uslova za " " i 9,kasnije mozda izbacim,PROVJERITI JOS SLUCAJEVA!
                                counterOfPlusAndSpace++;
                        }
                        if (counterOfPlusAndSpace != matchsec[i].Groups[3].Length)
                        {
                            MatchCollection msubfirstChar = Regex.Matches(matchsec[i].Groups[3].Value, @"\s*\+?\(*\d?");
                            if (!msubfirstChar[0].Value.Contains("+") && i > 1)
                            {
                                Console.WriteLine("Pogresno definisan aritmeticki izraz,fali PLUS na pocetku->{0}", str);
                                return true;
                            }
                            else
                            {
                                varOb.strings += " ";
                            }
                            try
                            {
                                varOb.strings += Evaluate(matchsec[i].Groups[3].Value + "+0").ToString();
                            }
                            catch (System.Data.SyntaxErrorException)
                            {
                                Console.WriteLine("Pogresno definisan aritmeticki izraz2 ->{0}", str);
                                return true;
                            }

                            MatchCollection msub = Regex.Matches(matchsec[i].Groups[3].Value, @"\)?\s*\+");
                            if (!msub[msub.Count - 1].Value.Contains("+"))
                            {
                                Console.WriteLine("Pogresno definisan aritmeticki izraz,fali PLUS na kraju->{0}", str);
                                return true;
                            }
                            else
                            {
                                varOb.strings += " ";
                            }
                        }
                    }
                    counterOfPlusAndSpace = 0;
                    if (counterOfPlus >= 2)
                    {
                        Console.WriteLine("Fali plus izmedju->{0}", str);
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine("Mora poceti sa [var]->{0}", str);
                    return true;
                }
            }
            if (DoesThisVarExist(varOb, arrayList))
            {
                Console.WriteLine("Ne moze doci do promjene tipa variable->{0}", str);
                return true;
            }
            else
            {
                //Console.WriteLine(varOb.strings); //jos trebam da dodam slucaj kada je upitanju string
                return false;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            TextValid textValid = new TextValid();
            ArrayList arrayList = new ArrayList();
            int decissonNumb = 0;
            try
            {
                using (StreamReader reader = new StreamReader("Code.txt")) //whe 0-element
                {
                    string str = "";
                    string oprtAritm = @"\[([\w\d\.\$\#_]+)\]\s*\=\s*|([\d\(\)\+\-\/\*\s]+)|\[([\w\d\.\$\#_]+)\]";
                    string oprtAritamAndString = @"\[([\w\d\.\$\#_]+)\]\s*\=\s*|" + "\u0022" + @"([\w\d\s\.\$\#_]+)" + "\u0022" + @"|([\d\(\)\+\-\/\*\s]+|\[([\w\d\.\$\#_]+)\])";

                    while ((str = reader.ReadLine()) != null)
                    {
                        decissonNumb = textValid.checkForInvalidChar(str); //checks if string contains unregulat characters
                        if (decissonNumb != 0) break;

                        decissonNumb = textValid.BraceCheck(str);//checks if the braces [] are ok
                        if (decissonNumb != 0) break;

                        decissonNumb = textValid.AllCapsCheck(str);//checks if the <all_caps> are ok
                        if (decissonNumb != 0) break;

                        MatchCollection matchfir = Regex.Matches(str, oprtAritm); //za provjeru aritmetickih variabli
                        string collectorOfFirstRegex = "", collectorOfSecondRegex = "";
                        foreach (Match mfir in matchfir)
                        {
                            collectorOfFirstRegex += mfir.ToString();
                        }
                        MatchCollection matchsec = Regex.Matches(str, oprtAritamAndString); //za provjeru slozenog izraza
                        foreach (Match m in matchsec)
                        {
                            collectorOfSecondRegex += m.ToString();
                        }

                        if (str.Equals(collectorOfFirstRegex))
                        {
                            decissonNumb = Convert.ToInt32(textValid.ArithmeticVarCheck(str, matchfir, arrayList));
                            if (decissonNumb != 0) break;
                        }
                        else if (str.Equals(collectorOfSecondRegex))
                        {
                            decissonNumb = Convert.ToInt32(textValid.ComplexVarCheck(str, matchsec, arrayList));
                            if (decissonNumb != 0) break;
                        }
                        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                        string oprtInlineBase = @"\@\{(.*?)\}";
                        ArrayList arrayOfBaseInlinestrings = new ArrayList();
                        string stringarrayOfBaseInlinestrings = "";
                        MatchCollection matchInlineBase = Regex.Matches(str, oprtInlineBase); //prepostavimo da su {} zagrade dobre i "" i @,napisati funkciju koja ce to provjeravati
                        foreach (Match m in matchInlineBase)
                        {
                            stringarrayOfBaseInlinestrings += m.Groups[1].Value;
                        }

                        string oprtInlineAdvenc = @"\@\{\s*|(" + "\u0022" + @"([\w\d\s\.\$\#_]+)" + "\u0022" + @"|([\d\(\)\+\-\/\*\s]+)|\[([\w\d\.\$\#_]+)\])|\s*\}";
                        ArrayList arrayOfAdvencInlineStrings = new ArrayList();
                        string stringOfAdvencInlineStrings = "";
                        int des = 0;
                        MatchCollection matchInlineAvanc = Regex.Matches(str, oprtInlineAdvenc);
                        foreach (Match m in matchInlineAvanc)
                        {
                            if (m.Value.Contains("@{"))
                                des = 1;
                            else if (m.Value.Contains("}"))
                                des = 0;
                            if (des != 0)
                                stringOfAdvencInlineStrings += m.Groups[1].Value;
                        }


                        if (!stringarrayOfBaseInlinestrings.Equals(stringOfAdvencInlineStrings))
                        {
                            Console.WriteLine("Doslo je do greske u <izraz>->{0}", str);
                            break;
                        }



                        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                        string oprtPutingVar = @"\s*\=\[([\w\d\.\$\#_]+)\]|\[([\w\d\.\$\#_]+)\]";
                        MatchCollection matchthired = Regex.Matches(str, oprtPutingVar); //za zamjenu [var] sa pravim vrijednostima
                        foreach (Match mthir in matchthired)
                        {
                            if (mthir.Groups[2].Value != "" && !Regex.IsMatch(str, @"\[([\w\d\.\$\#_]+)\]\s*\=\s*"))
                            {
                                Console.WriteLine("Ne pravilno definisan ispis variable->{0}", str);
                                break;
                            }
                            else if (mthir.Groups[1].Value != "")
                            {
                                foreach (Variable var in arrayList)
                                {
                                    if (var.nameOfVar == mthir.Groups[1].Value)
                                    {
                                        str = str.Replace("[" + var.nameOfVar + "]", var.strings);
                                    }
                                }
                            }

                        }


                        Console.WriteLine(str);
                        //ovde treba dodati da se prom ubaci ako treba i onda da se izvrsi ToUpper
                        str = textValid.ToUpper(str); //converts lowCase to UperCase and removes <all_caps> and </all_caps>
                                                      // Console.WriteLine("{0}", str);
                                                      // Console.WriteLine(holeText);
                    }
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
            }
            Console.ReadLine();
        }
    }
}
