using ClosedXML.Excel;
using NUnit.Framework;
using System;
using ClosedXML.Excel.CalcEngine;

namespace ClosedXML.Tests.Excel.CalcEngine
{
    [TestFixture]
    [SetCulture("en-US")]
    public class InformationTests
    {
        [TestCase("A1")] // blank
        [TestCase("TRUE")]
        [TestCase("14.5")]
        [TestCase("\"text\"")]
        public void ErrorType_NonErrorsAreNA(string argumentFormula)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            Assert.AreEqual(XLError.NoValueAvailable, ws.Evaluate($"ERROR.TYPE({argumentFormula})"));
        }

        [TestCase("#NULL!",1)]
        [TestCase("#DIV/0!", 2)]
        [TestCase("#VALUE!", 3)]
        [TestCase("#REF!", 4)]
        [TestCase("#NAME?", 5)]
        [TestCase("#NUM!", 6)]
        [TestCase("#N/A", 7)]
        //[TestCase("#GETTING_DATA", 8)] OLAP Cube not supported
        public void ErrorType_ReturnsNumberForError(string error, int expectedNumber)
        {
            Assert.AreEqual(expectedNumber, XLWorkbook.EvaluateExpr($"ERROR.TYPE({error})"));
        }

        #region IsBlank Tests

        [Test]
        public void IsBlank_EmptyCell_true()
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            var actual = ws.Evaluate("IsBlank(A1)");
            Assert.AreEqual(true, actual);
        }

        [Test]
        public void IsBlank_NonEmptyCell_false()
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            ws.Cell("A1").Value = "1";
            var actual = ws.Evaluate("IsBlank(A1)");
            Assert.AreEqual(false, actual);
        }

        [TestCase("FALSE")]
        [TestCase("0")]
        [TestCase("5")]
        [TestCase("\"\"")]
        [TestCase("\"Hello\"")]
        [TestCase("#DIV/0!")]
        public void IsBlank_NonEmptyValue_false(string value)
        {
            var actual = XLWorkbook.EvaluateExpr($"IsBlank({value})");
            Assert.AreEqual(false, actual);
        }

        [Test]
        public void IsBlank_InlineBlank_true()
        {
            var actual = XLWorkbook.EvaluateExpr("IsBlank(IF(TRUE,,))");
            Assert.AreEqual(true, actual);
        }

        #endregion IsBlank Tests

        [TestCase("IF(TRUE,,)")]
        [TestCase("FALSE")]
        [TestCase("0")]
        [TestCase("\"\"")]
        [TestCase("\"text\"")]
        public void IsErr_NonErrorValues_false(string valueFormula)
        {
            var actual = XLWorkbook.EvaluateExpr($"IsErr({valueFormula})");
            Assert.AreEqual(false, actual);
        }

        [TestCase("#DIV/0!")]
        [TestCase("#NAME?")]
        [TestCase("#NULL!")]
        [TestCase("#NUM!")]
        [TestCase("#REF!")]
        [TestCase("#VALUE!")]
        public void IsErr_ErrorsExceptNA_true(string valueFormula)
        {
            var actual = XLWorkbook.EvaluateExpr($"IsErr({valueFormula})");
            Assert.AreEqual(true, actual);
        }

        [Test]
        public void IsErr_NA_false()
        {
            var actual = XLWorkbook.EvaluateExpr("IsErr(#N/A)");
            Assert.AreEqual(false, actual);
        }

        #region IsEven Tests

        [Test]
        public void IsEven_Single_False()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet");

                ws.Cell("A1").Value = 1;
                ws.Cell("A2").Value = 1.2;
                ws.Cell("A3").Value = 3;

                var actual = ws.Evaluate("=IsEven(A1)");
                Assert.AreEqual(false, actual);

                actual = ws.Evaluate("=IsEven(A2)");
                Assert.AreEqual(false, actual);

                actual = ws.Evaluate("=IsEven(A3)");
                Assert.AreEqual(false, actual);
            }
        }

        [Test]
        public void IsEven_Single_True()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet");

                ws.Cell("A1").Value = 4;
                ws.Cell("A2").Value = 0.2;
                ws.Cell("A3").Value = 12.2;

                var actual = ws.Evaluate("=IsEven(A1)");
                Assert.AreEqual(true, actual);

                actual = ws.Evaluate("=IsEven(A2)");
                Assert.AreEqual(true, actual);

                actual = ws.Evaluate("=IsEven(A3)");
                Assert.AreEqual(true, actual);
            }
        }

        #endregion IsEven Tests

        #region IsLogical Tests

        [Test]
        public void IsLogical_Simpe_False()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet");

                ws.Cell("A1").Value = 123;

                var actual = ws.Evaluate("=IsLogical(A1)");
                Assert.AreEqual(false, actual);
            }
        }

        [Test]
        public void IsLogical_Simple_True()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet");

                ws.Cell("A1").Value = true;

                var actual = ws.Evaluate("=IsLogical(A1)");
                Assert.AreEqual(true, actual);
            }
        }

        #endregion IsLogical Tests

        [Test]
        public void IsNA()
        {
            object actual;
            actual = XLWorkbook.EvaluateExpr("ISNA(#N/A)");
            Assert.AreEqual(true, actual);

            actual = XLWorkbook.EvaluateExpr("ISNA(#REF!)");
            Assert.AreEqual(false, actual);
        }

        #region IsNotText Tests

        [Test]
        public void IsNotText_Simple_false()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet");
                ws.Cell("A1").Value = "asd";
                var actual = ws.Evaluate("=IsNonText(A1)");
                Assert.AreEqual(false, actual);
            }
        }

        [Test]
        public void IsNotText_Simple_true()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet");
                ws.Cell("A1").Value = "123"; //Double Value
                ws.Cell("A2").Value = DateTime.Now; //Date Value
                ws.Cell("A3").Value = "12,235.5"; //Comma Formatting
                ws.Cell("A4").Value = "$12,235.5"; //Currency Value
                ws.Cell("A5").Value = true; //Bool Value
                ws.Cell("A6").Value = "12%"; //Percentage Value

                var actual = ws.Evaluate("=IsNonText(A1)");
                Assert.AreEqual(true, actual);
                actual = ws.Evaluate("=IsNonText(A2)");
                Assert.AreEqual(true, actual);
                actual = ws.Evaluate("=IsNonText(A3)");
                Assert.AreEqual(true, actual);
                actual = ws.Evaluate("=IsNonText(A4)");
                Assert.AreEqual(true, actual);
                actual = ws.Evaluate("=IsNonText(A5)");
                Assert.AreEqual(true, actual);
                actual = ws.Evaluate("=IsNonText(A6)");
                Assert.AreEqual(true, actual);
            }
        }

        #endregion IsNotText Tests

        #region IsNumber Tests

        [Test]
        public void IsNumber_Simple_false()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet");
                ws.Cell("A1").Value = "asd"; //String Value
                ws.Cell("A2").Value = true; //Bool Value

                var actual = ws.Evaluate("=IsNumber(A1)");
                Assert.AreEqual(false, actual);
                actual = ws.Evaluate("=IsNumber(A2)");
                Assert.AreEqual(false, actual);
            }
        }

        [Test]
        public void IsNumber_Simple_true()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet");
                ws.Cell("A1").Value = "123"; //Double Value
                ws.Cell("A2").Value = DateTime.Now; //Date Value
                ws.Cell("A3").Value = "12,235.5"; //Coma Formatting
                ws.Cell("A4").Value = "$12,235.5"; //Currency Value
                ws.Cell("A5").Value = "12%"; //Percentage Value

                var actual = ws.Evaluate("=IsNumber(A1)");
                Assert.AreEqual(true, actual);
                actual = ws.Evaluate("=IsNumber(A2)");
                Assert.AreEqual(true, actual);
                actual = ws.Evaluate("=IsNumber(A3)");
                Assert.AreEqual(true, actual);
                actual = ws.Evaluate("=IsNumber(A4)");
                Assert.AreEqual(true, actual);
                actual = ws.Evaluate("=IsNumber(A5)");
                Assert.AreEqual(true, actual);
            }
        }

        #endregion IsNumber Tests

        #region IsOdd Test

        [Test]
        public void IsOdd_Simple_false()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet");

                ws.Cell("A1").Value = 4;
                ws.Cell("A2").Value = 0.2;
                ws.Cell("A3").Value = 12.2;

                var actual = ws.Evaluate("=IsOdd(A1)");
                Assert.AreEqual(false, actual);
                actual = ws.Evaluate("=IsOdd(A2)");
                Assert.AreEqual(false, actual);
                actual = ws.Evaluate("=IsOdd(A3)");
                Assert.AreEqual(false, actual);
            }
        }

        [Test]
        public void IsOdd_Simple_true()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet");

                ws.Cell("A1").Value = 1;
                ws.Cell("A2").Value = 1.2;
                ws.Cell("A3").Value = 3;

                var actual = ws.Evaluate("=IsOdd(A1)");
                Assert.AreEqual(true, actual);
                actual = ws.Evaluate("=IsOdd(A2)");
                Assert.AreEqual(true, actual);
                actual = ws.Evaluate("=IsOdd(A3)");
                Assert.AreEqual(true, actual);
            }
        }

        #endregion IsOdd Test

        [Test]
        public void IsRef()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet");
                ws.Cell("A1").Value = "123";

                ws.Cell("B1").FormulaA1 = "ISREF(A1)";
                ws.Cell("B2").FormulaA1 = "ISREF(5)";
                ws.Cell("B3").FormulaA1 = "ISREF(YEAR(TODAY()))";

                XLCellValue actual;
                actual = ws.Cell("B1").Value;
                Assert.AreEqual(true, actual);

                actual = ws.Cell("B2").Value;
                Assert.AreEqual(false, actual);

                actual = ws.Cell("B3").Value;
                Assert.AreEqual(false, actual);
            }
        }

        #region IsText Tests

        [Test]
        public void IsText_Simple_false()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet");
                ws.Cell("A1").Value = "123"; //Double Value
                ws.Cell("A2").Value = DateTime.Now; //Date Value
                ws.Cell("A3").Value = "12,235.5"; //Comma Formatting
                ws.Cell("A4").Value = "$12,235.5"; //Currency Value
                ws.Cell("A5").Value = true; //Bool Value
                ws.Cell("A6").Value = "12%"; //Percentage Value

                var actual = ws.Evaluate("=IsText(A1)");
                Assert.AreEqual(false, actual);
                actual = ws.Evaluate("=IsText(A2)");
                Assert.AreEqual(false, actual);
                actual = ws.Evaluate("=IsText(A3)");
                Assert.AreEqual(false, actual);
                actual = ws.Evaluate("=IsText(A4)");
                Assert.AreEqual(false, actual);
                actual = ws.Evaluate("=IsText(A5)");
                Assert.AreEqual(false, actual);
                actual = ws.Evaluate("=IsText(A6)");
                Assert.AreEqual(false, actual);
            }
        }

        [Test]
        public void IsText_Simple_true()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet");

                ws.Cell("A1").Value = "asd";

                var actual = ws.Evaluate("=IsText(A1)");
                Assert.AreEqual(true, actual);
            }
        }

        #endregion IsText Tests

        #region N Tests

        [Test]
        public void N_Date_SerialNumber()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet");
                var testedDate = DateTime.Now;
                ws.Cell("A1").Value = testedDate;
                var actual = ws.Evaluate("=N(A1)");
                Assert.AreEqual(testedDate.ToOADate(), actual);
            }
        }

        [Test]
        public void N_False_Zero()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet");
                ws.Cell("A1").Value = false;
                var actual = ws.Evaluate("=N(A1)");
                Assert.AreEqual(0, actual);
            }
        }

        [Test]
        public void N_Number_Number()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet");
                var testedValue = 123;
                ws.Cell("A1").Value = testedValue;
                var actual = ws.Evaluate("=N(A1)");
                Assert.AreEqual(testedValue, actual);
            }
        }

        [Test]
        public void N_String_Zero()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet");
                ws.Cell("A1").Value = "asd";
                var actual = ws.Evaluate("=N(A1)");
                Assert.AreEqual(0, actual);
            }
        }

        [Test]
        public void N_True_One()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet");
                ws.Cell("A1").Value = true;
                var actual = ws.Evaluate("=N(A1)");
                Assert.AreEqual(1, actual);
            }
        }

        #endregion N Tests
    }
}
