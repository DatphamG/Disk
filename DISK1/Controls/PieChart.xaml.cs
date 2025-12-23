using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DISK1.Controls
{
    public partial class PieChart : UserControl
    {
        public PieChart()
        {
            InitializeComponent();
        }

        public void Draw(IEnumerable<PieSlice> slices)
        {
            canvasChart.Children.Clear();
            stackLegend.Children.Clear();

            double total = slices.Sum(s => s.Value);
            if (total <= 0) return;

            double startAngle = -90; // Start at 12 o'clock
            Point center = new Point(100, 100);
            double radius = 100;

            foreach (var slice in slices)
            {
                double sweepAngle = (slice.Value / total) * 360;
                
                // Draw Slice
                if (sweepAngle > 359.9) 
                {
                    // Full circle
                    var ellipse = new Ellipse
                    {
                        Width = radius * 2,
                        Height = radius * 2,
                        Fill = slice.Color,
                        ToolTip = $"{slice.Label}: {slice.FormattedValue}"
                    };
                    canvasChart.Children.Add(ellipse);
                }
                else
                {
                    var path = new Path
                    {
                        Fill = slice.Color,
                        ToolTip = $"{slice.Label}\n{slice.FormattedValue} ({Math.Round(slice.Value/total*100, 1)}%)"
                    };

                    PathGeometry geometry = new PathGeometry();
                    PathFigure figure = new PathFigure { StartPoint = center };

                    // Calculate end points
                    double endAngle = startAngle + sweepAngle;
                    
                    // First line outward to start point on circumference
                    Point startPointOnCircumference = ComputePointOnCircle(center, radius, startAngle);
                                        
                    figure.Segments.Add(new LineSegment(startPointOnCircumference, true));

                    // Arc
                    Point endPointOnCircumference = ComputePointOnCircle(center, radius, endAngle);
                    figure.Segments.Add(new ArcSegment(
                        endPointOnCircumference,
                        new Size(radius, radius),
                        0,
                        sweepAngle > 180,
                        SweepDirection.Clockwise,
                        true));

                    // Close the figure back to center
                    figure.IsClosed = true;
                    geometry.Figures.Add(figure);
                    path.Data = geometry;

                    canvasChart.Children.Add(path);
                }

                // Add Legend Item
                var legendItem = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 5) };
                legendItem.Children.Add(new Border 
                { 
                    Width = 12, 
                    Height = 12, 
                    Background = slice.Color, 
                    CornerRadius = new CornerRadius(2),
                    Margin = new Thickness(0, 0, 8, 0)
                });
                
                var textBlock = new TextBlock 
                { 
                    Text = slice.Label, 
                    Foreground = Brushes.White, 
                    FontSize = 11,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    ToolTip = slice.Label
                };
                legendItem.Children.Add(textBlock);
                stackLegend.Children.Add(legendItem);

                startAngle += sweepAngle;
            }
        }

        private Point ComputePointOnCircle(Point center, double radius, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * Math.PI / 180.0;
            return new Point(
                center.X + radius * Math.Cos(angleInRadians),
                center.Y + radius * Math.Sin(angleInRadians));
        }
    }
}
