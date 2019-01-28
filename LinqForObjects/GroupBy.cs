using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNDStudios.Tutorials.LinqForObjects
{
    /// <summary>
    /// Test object for timesheet lines to show how to group 
    /// objects with Linq
    /// </summary>
    public class TimesheetLine
    {
        public DateTime Day { get; set; }
        public String RateCode { get; set; }
        public String RateDescription { get; set; }
        public Decimal Rate { get; set; }
        public Decimal Volume { get; set; }
    }

    /// <summary>
    /// Test fixture to run some of the linq grouping examples
    /// </summary>
    [TestFixture]
    public class GroupBy
    {
        /// <summary>
        /// Test data full of timesheet lines so that the 
        /// grouping tests can be run as examples
        /// </summary>
        private List<TimesheetLine> lines = new List<TimesheetLine>()
            {
                new TimesheetLine(){ Day = new DateTime(2019, 1, 1) , Rate = (Decimal)21.00, RateCode = "ST", RateDescription = "Standard Rate", Volume = (Decimal)20.2 },
                new TimesheetLine(){ Day = new DateTime(2019, 1, 1) , Rate = (Decimal)21.00, RateCode = "ST", RateDescription = "Standard Rate", Volume = (Decimal)10.1 },
                new TimesheetLine(){ Day = new DateTime(2019, 1, 2) , Rate = (Decimal)21.00, RateCode = "ST", RateDescription = "Standard Rate", Volume = (Decimal)20.2 },
                new TimesheetLine(){ Day = new DateTime(2019, 1, 2) , Rate = (Decimal)31.00, RateCode = "OV", RateDescription = "Overtime Rate", Volume = (Decimal)30.99 }
            };
        
        /// <summary>
        /// Test to show grouping of data and that the result is of the
        /// expected grouped value, grouped by rate codes
        /// </summary>
        [Test]
        public void GroupBy_Test()
        {
            // Arrange
            
            // Act
            var groupedLines = lines
                .GroupBy(grp => grp.RateCode)
                .Select(dat =>
                        new
                        {
                            RateCode = dat.Key,
                            Volume = (Decimal)dat.Sum(line => line.Volume),
                            RateDescription = dat.FirstOrDefault().RateDescription,
                            Rate = (Decimal)dat.FirstOrDefault().Rate
                        }
                    )
                .ToList();
            
            // Assert
            Assert.True(groupedLines.Count == 2); // Right amount of lines?
        }

        /// <summary>
        /// Test to show grouping of data that has a sub-grouping
        /// Grouped by days then by rate type (with rates of the same type
        /// by day summed together)
        /// </summary>
        [Test]
        public void GroupBy_GroupBy_Test()
        {
            // Arrange

            // Act

            // Group by the days in the data, then group by the unique lines
            // of data by each day, then sum up the total volume per each unique
            // rate type
            var groupedLines = lines
                .GroupBy(grp => grp.Day)
                .Select(dat =>
                        new
                        {
                            Day = dat.Key,
                            Lines = dat.GroupBy(subGroup => subGroup.RateCode)
                                    .Select(sub =>
                                        new
                                        {
                                            RateCode = sub.Key,
                                            Rate = (Decimal)sub.FirstOrDefault().Rate,
                                            Volume = (Decimal)sub.Sum(line => line.Volume),
                                            RateDescription = sub.FirstOrDefault().RateDescription
                                        }
                                    )
                                .ToList()
                        }
                    )
                .ToList();

            // Build up some HTML to show how the grouping works
            // starting with the header
            StringBuilder linesBuilder = new StringBuilder("<table border=\"1\" cellspacing=\"1\" cellpadding=\"1\">");
            linesBuilder.AppendLine("<thead>");
            linesBuilder.AppendLine("<th>Day</th>");
            linesBuilder.AppendLine("<th>Rate Code</th>");
            linesBuilder.AppendLine("<th>Rate Description</th>");
            linesBuilder.AppendLine("<th>Rate</th>");
            linesBuilder.AppendLine("<th>Volume</th>");
            linesBuilder.AppendLine("</thead>");
            linesBuilder.AppendLine("<tbody>");

            // Loop the days in the result
            groupedLines.ForEach(day =>
            {
                // Loop the lines in each day
                Int32 lineCounter = 0;
                day.Lines.ForEach(line => 
                {
                    // Start a new row
                    linesBuilder.AppendLine($"<tr>");

                    // For the first line only, indicate the day but row span the data
                    // over the amount of rates for this day (so the cell only shows once)
                    if (lineCounter == 0)
                        linesBuilder.AppendLine($"<td style=\"vertical-align:top\" rowspan=\"{day.Lines.Count.ToString()}\">{day.Day.ToString("D")}</td>");

                    // Append the main rate data
                    linesBuilder.AppendLine($"<td>{line.RateCode}</td>");
                    linesBuilder.AppendLine($"<td>{line.RateDescription}</td>");
                    linesBuilder.AppendLine($"<td>{line.Rate.ToString("N2")}</td>");
                    linesBuilder.AppendLine($"<td>{line.Volume.ToString("N2")}</td>");

                    // Close the row
                    linesBuilder.AppendLine("</tr>");

                    // Indicate we have moved to the next line
                    lineCounter++; 
                });
            });

            linesBuilder.AppendLine("</tbody></table>"); // Close the table

            // Assert
            Assert.True(groupedLines.Count == 2); // Right amount of days?
            Assert.Greater(linesBuilder.Length, 0); // Some html written out?
            //File.WriteAllText(@"C:\test.htm", linesBuilder.ToString());
        }

    }
}
