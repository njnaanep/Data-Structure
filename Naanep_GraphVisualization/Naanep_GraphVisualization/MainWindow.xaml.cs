using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Graph;
using Naanep_GraphVisualization.Annotations;
using static System.String;
using static System.Windows.Visibility;
using Path = Graph.Path;

namespace Naanep_GraphVisualization
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            LsBoxVertices.ItemsSource = Vertices;

            CmbEditVertex.ItemsSource = Vertices;

            CmbFromVertex.ItemsSource = Vertices;
            CmbToVertex.ItemsSource = Vertices;

            CmbShortestFrom.ItemsSource = Vertices;
            CmbShortestTo.ItemsSource = Vertices;
        }

        #region DataStorage

        private static readonly IList<string> Vertices = new List<string>();

        private static readonly IList<WeightedEdge> Edges = new List<WeightedEdge>();

        private static readonly IList<Point> VerticesCoordinates = new List<Point>();

        private static readonly IList<VisualVertex> VisualVertices = new List<VisualVertex>();

        private static readonly IList<VisualEdges> VisualEdge = new List<VisualEdges>();
        
        private int _vertexIndex=1;

        #endregion

        #region Graph

        private static WeightedGraph<string> _graph;
        private Path _shortestPath;
        private Stack<int> _shortestDestinationPath;

        #endregion

        #region VertexOption

        private void CnvGraph_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (TabOption.SelectedIndex== 1)
            {
                TxtXCoordinate.Text = e.GetPosition(CnvGraph).X.ToString();
                TxtYCoordinate.Text = e.GetPosition(CnvGraph).Y.ToString();

                BtnAddVertex.IsEnabled = true;
                CnvGraph.IsHitTestVisible = false;
            }

            if (TabOption.SelectedIndex == 2)
            {
                if (EditTab.SelectedIndex == 1)
                {
                    if (CmbEditVertex.SelectedIndex == -1)
                        return;

                    CnvGraph.IsHitTestVisible = false;

                    TempEllipse.Visibility = Collapsed;

                    VisualVertices[CmbEditVertex.SelectedIndex].Ellipse.Visibility = Visible;
                    VisualVertices[CmbEditVertex.SelectedIndex].Label.Visibility = Visible;

                    //ellipse
                    Canvas.SetLeft(VisualVertices[CmbEditVertex.SelectedIndex].Ellipse, e.GetPosition(CnvGraph).X - 15);
                    Canvas.SetTop(VisualVertices[CmbEditVertex.SelectedIndex].Ellipse, e.GetPosition(CnvGraph).Y - 15);

                    //label
                    Canvas.SetLeft(VisualVertices[CmbEditVertex.SelectedIndex].Label, e.GetPosition(CnvGraph).X - 10);
                    Canvas.SetTop(VisualVertices[CmbEditVertex.SelectedIndex].Label, e.GetPosition(CnvGraph).Y - 15);

                    //edge
                    for (var i = 0; i < VisualEdge.Count; i++)
                    {
                        if (VerticesCoordinates[CmbEditVertex.SelectedIndex] == VisualEdge[i].Edge.Points[0])
                        {
                            VisualEdge[i].Edge.Points[0] = e.GetPosition(CnvGraph);

                            Canvas.SetLeft(VisualEdge[i].Label, MidPoint(VisualEdge[i].Edge.Points[0], VisualEdge[i].Edge.Points[1]).X);
                            Canvas.SetTop(VisualEdge[i].Label, MidPoint(e.GetPosition(CnvGraph), VisualEdge[i].Edge.Points[1]).Y);
                        }
                        else if (VerticesCoordinates[CmbEditVertex.SelectedIndex] == VisualEdge[i].Edge.Points[1])
                        {
                            VisualEdge[i].Edge.Points[1] = e.GetPosition(CnvGraph);

                            Canvas.SetLeft(VisualEdge[i].Label, MidPoint(VisualEdge[i].Edge.Points[1], e.GetPosition(CnvGraph)).X);
                            Canvas.SetTop(VisualEdge[i].Label, MidPoint(VisualEdge[i].Edge.Points[1], e.GetPosition(CnvGraph)).Y);
                        }
                    }


                    VerticesCoordinates[CmbEditVertex.SelectedIndex] = e.GetPosition(CnvGraph);
                    
                    for (var i = 0; i < VisualEdge.Count; i++)
                        if (VisualEdge[i].Edge.Points.Contains(VerticesCoordinates[CmbEditVertex.SelectedIndex]))
                        {
                            VisualEdge[i].Edge.Visibility = Visible;
                            VisualEdge[i].Label.Visibility = Visible;
                        }

                    MovePanel.Visibility = Collapsed;

                    CmbEditVertex.SelectedIndex = -1;

                    BtnTransferEdit.IsEnabled = false;
                }
            }
        }

        private void CnvGraph_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (TabOption.SelectedIndex == 1)
            {
                TxtXCoordinate.Text = e.GetPosition(CnvGraph).X.ToString();
                TxtYCoordinate.Text = e.GetPosition(CnvGraph).Y.ToString();


                TempEllipse.Visibility = Visible;

                Canvas.SetLeft(TempEllipse,e.GetPosition(CnvGraph).X - 15);
                Canvas.SetTop(TempEllipse, e.GetPosition(CnvGraph).Y - 15);
            }

            if (TabOption.SelectedIndex == 2)
            {
                if (EditTab.SelectedIndex == 1)
                {
                    if (CmbEditVertex.SelectedIndex == -1)
                        return;

                    VisualVertices[CmbEditVertex.SelectedIndex].Ellipse.Visibility = Collapsed;
                    VisualVertices[CmbEditVertex.SelectedIndex].Label.Visibility = Collapsed;

                    TempEllipse.Visibility = Visible;

                    TxtBNewXCoordinates.Text = e.GetPosition(CnvGraph).X.ToString();
                    TxtBNewYCoordinates.Text = e.GetPosition(CnvGraph).Y.ToString();

                    Canvas.SetLeft(TempEllipse, e.GetPosition(CnvGraph).X -15);
                    Canvas.SetTop(TempEllipse, e.GetPosition(CnvGraph).Y -15);
                }

                for (var i = 0; i < VisualEdge.Count; i++)
                {
                    if (VisualEdge[i].Edge.Points.Contains(VerticesCoordinates[CmbEditVertex.SelectedIndex]))
                    {
                        VisualEdge[i].Edge.Visibility = Collapsed;
                        VisualEdge[i].Label.Visibility = Collapsed;
                    }
                }
            }
        }

        private void BtnVertex_Click(object sender, RoutedEventArgs e)
        {
            TabOption.SelectedIndex = 1;

            CnvGraph.IsHitTestVisible = true;

            ClearHighlight();
            ShortestPathLine.Visibility = Collapsed;
            TempEllipse.Visibility = Collapsed;

            LabelCost.Visibility = Visibility.Collapsed;
            TxtBlockCost.Visibility = Visibility.Collapsed;
            LabelResult.Visibility = Visibility.Collapsed;
            TxtBlockResult.Visibility = Visibility.Collapsed;
            ShortestPathLine.Visibility = Visibility.Collapsed;

            BtnGetResult.Visibility = Visibility.Visible;
        }

        private void BtnEdge_Click(object sender, RoutedEventArgs e)
        {
            TabOption.SelectedIndex = 3;

            ClearHighlight();

            LabelCost.Visibility = Visibility.Collapsed;
            TxtBlockCost.Visibility = Visibility.Collapsed;
            LabelResult.Visibility = Visibility.Collapsed;
            TxtBlockResult.Visibility = Visibility.Collapsed;
            ShortestPathLine.Visibility = Visibility.Collapsed;

            TempEllipse.Visibility = Collapsed;


            BtnGetResult.Visibility = Visibility.Visible;
        }

        private void BtnAddVertex_OnClick(object sender, RoutedEventArgs e)
        {
            CreateVertex(TxtXCoordinate,TxtYCoordinate,TxtVertexName);

            TxtVertexName.Clear();
            TxtXCoordinate.Clear();
            TxtYCoordinate.Clear();
            CnvGraph.IsHitTestVisible = true;

            RefreshItems();

            BtnAddVertex.IsEnabled = false;
            BtnEdit.IsEnabled = true;

            TempEllipse.Visibility = Collapsed;

            _vertexIndex++;
        }
        
        private void BtnResetVertex_OnClick(object sender, RoutedEventArgs e)
        {
            TxtXCoordinate.Text = Empty;
            TxtYCoordinate.Text = Empty;

            CnvGraph.IsHitTestVisible = true;
        }



        #endregion

        #region EdgeOption

        private void TextboxNumericInputOnly(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text); //text box only accept numbers
        }

        private void BtnAddEdge_OnClick(object sender, RoutedEventArgs e)
        {
            if (CmbFromVertex.SelectedIndex == CmbToVertex.SelectedIndex)
            {
                MessageBox.Show("Cannot add edge to self");
                return;
            }


            Edges.Add(new WeightedEdge(fromVertex: CmbFromVertex.SelectedIndex, 
                                                toVertex: CmbToVertex.SelectedIndex, 
                                                weight: Convert.ToDouble(TxtWeight.Text)));

            if(RBtnUndirected.IsChecked==true)
                Edges.Add(new WeightedEdge(fromVertex: CmbToVertex.SelectedIndex, 
                                                    toVertex: CmbFromVertex.SelectedIndex,
                                                    weight: Convert.ToDouble(TxtWeight.Text)));

            CreateEdge(CmbFromVertex, CmbToVertex);

            CmbFromVertex.SelectedIndex = -1;
            CmbToVertex.SelectedIndex = -1;
            TxtWeight.Text = Empty;

            CmbShortestFrom.IsEnabled = true;
            CmbShortestTo.IsEnabled = true;
            BtnGetResult.IsEnabled = true;

            RBtnUndirected.IsChecked = true;
            BtnAddEdge.IsEnabled = false;

            RefreshItems();
        }

        private void CmbFromVertex_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbToVertex.SelectedIndex != -1 && !IsNullOrEmpty(TxtWeight.Text))
            {
                BtnAddEdge.IsEnabled = true;
            }
        }

        private void CmbToVertex_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbFromVertex.SelectedIndex != -1 && !IsNullOrEmpty(TxtWeight.Text))
            {
                BtnAddEdge.IsEnabled = true;
            }
        }

        private void TxtWeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CmbToVertex.SelectedIndex != -1 && CmbFromVertex.SelectedIndex != -1)
            {
                BtnAddEdge.IsEnabled = true;
            }
        }

        private void RBtnDirected_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RBtnUndirected.IsChecked = false;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void RBtnUndirected_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RBtnDirected.IsChecked = false;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #endregion

        #region Result

        private void CmbShortestSelection(object sender, SelectionChangedEventArgs e)
        {
            LabelCost.Visibility = Visibility.Collapsed;
            TxtBlockCost.Visibility = Visibility.Collapsed;
            LabelResult.Visibility = Visibility.Collapsed;
            TxtBlockResult.Visibility = Visibility.Collapsed;
            ShortestPathLine.Visibility = Visibility.Collapsed;

            TempEllipse.Visibility = Collapsed;

            if (CmbShortestFrom.SelectedIndex != -1 || CmbShortestTo.SelectedIndex != -1)
            {
                BtnGetResult.IsEnabled = true;
            }

            TabOption.SelectedIndex = 0;

            ClearHighlight();

            BtnGetResult.Visibility = Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _graph = new WeightedGraph<string>(Edges, Vertices);

            _shortestPath = _graph.GetShortestPath(CmbShortestFrom.SelectedIndex);

            try
            {
                _shortestDestinationPath = _graph.ShortestPathDestination(_shortestPath, CmbShortestTo.SelectedIndex);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Vertices are not connected");

                CmbShortestTo.SelectedIndex = -1;
                CmbShortestFrom.SelectedIndex = -1;

                BtnGetResult.IsEnabled = false;

                return;
            }

            var sb = new StringBuilder();

            #region UI Element Visibility

            BtnGetResult.Visibility = Visibility.Collapsed;

            LabelCost.Visibility = Visibility.Visible;
            TxtBlockCost.Visibility = Visibility.Visible;
            LabelResult.Visibility = Visibility.Visible;
            TxtBlockResult.Visibility = Visibility.Visible;

            #endregion



            foreach (var i in _shortestDestinationPath) sb.Append($"{Vertices[i].Split(':')[0],2}");

            var points = new PointCollection();

            foreach (var point in _shortestDestinationPath) points.Add(VerticesCoordinates[point]);

            ShortestPathLine.Points = points;
            ShortestPathLine.Visibility = Visible;

            TxtBlockResult.Text = sb.ToString();
            TxtBlockCost.Text = _shortestPath.Distance[CmbShortestTo.SelectedIndex].ToString();
        }

        #endregion

        #region Classes

        public class VisualVertex
        {
            public Ellipse Ellipse { get; }
            public Label Label { get; }

            public VisualVertex(Ellipse ellipse, Label label)
            {
                Ellipse = ellipse;
                Label = label;
            }
        }

        public class VisualEdges
        {
            public Polyline Edge { get; }
            public Label Label { get; }

            public VisualEdges(Polyline edge, Label label)
            {
                Edge = edge;
                Label = label;
            }
        }

        #endregion

        #region Edit Vertices

        private void BtnDeleteEdit_OnClick(object sender, RoutedEventArgs e)
        {
            ClearHighlight();

            #region Remove UI Element from canvas

            CnvGraph.Children.Remove(VisualVertices[LsBoxVertices.SelectedIndex].Ellipse);
            CnvGraph.Children.Remove(VisualVertices[LsBoxVertices.SelectedIndex].Label);

            VisualVertices.RemoveAt(LsBoxVertices.SelectedIndex); //remove from visual storage

            for (var i = 0; i < VisualEdge.Count; i++)
            {
                if (VisualEdge[i].Edge.Points.Contains(VerticesCoordinates[LsBoxVertices.SelectedIndex]))
                {
                    CnvGraph.Children.Remove(VisualEdge[i].Edge);
                    CnvGraph.Children.Remove(VisualEdge[i].Label);

                    VisualEdge.RemoveAt(i);
                    i--; //prevent skipping after deleting
                }
            }

            #endregion

            #region Remove from data storage

            Vertices.RemoveAt(LsBoxVertices.SelectedIndex); //remove vertices list
            VerticesCoordinates.RemoveAt(LsBoxVertices.SelectedIndex); //remove points of vertices 

            //Remove Edges
            for (var i = 0; i < Edges.Count; i++)
                if (Edges[i].FromVertex == LsBoxVertices.SelectedIndex || Edges[i].ToVertex == LsBoxVertices.SelectedIndex)
                //check whether the edge contains the index of the vertices
                {
                    Edges.RemoveAt(i);
                    i--; //prevent skipping after deleting
                }

            #endregion

            #region UI Interaction
            CmbShortestFrom.SelectedIndex = -1;
            CmbShortestTo.SelectedIndex = -1;

            RefreshItems();
            ShortestPathLine.Visibility = Collapsed;

            BtnDelete.IsEnabled = false;

            if (Vertices.Count <= 0)
            {
                BtnEdit.IsEnabled = false;
                BtnDelete.IsEnabled = false;

                CmbShortestFrom.IsEnabled = false;
                CmbShortestTo.IsEnabled = false;

                BtnGetResult.IsEnabled = false;
            }
            #endregion

        }

        private void BtnRenameEdit_Click(object sender, RoutedEventArgs e)
        {
            var split = CmbEditVertex.SelectedItem.ToString().Split(':');

            Vertices[CmbEditVertex.SelectedIndex] = $"{split[0]}: {TxtEditName.Text}";

            CmbEditVertex.SelectedIndex = -1;

            TxtEditName.Text = string.Empty;

            RefreshItems();
        }

        private void BtnTransferEdit_Click(object sender, RoutedEventArgs e)
        {
            CnvGraph.IsHitTestVisible = true;
            MovePanel.Visibility = Visible;

            BtnTransferEdit.IsEnabled = false;
        }

        private void CmbEditVertex_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnTransferEdit.IsEnabled = true;
            
            BtnRenameEdit.IsEnabled = true;
        }


        #endregion
        
        #region Methods

        private void CreateVertex(TextBox x, TextBox y, TextBox name)
        {
            var xCoordinate = Convert.ToDouble(x.Text);
            var yCoordinate = Convert.ToDouble(y.Text);

            var ellipse = new Ellipse();
            Canvas.SetLeft(ellipse, xCoordinate - 15);
            Canvas.SetTop(ellipse, yCoordinate - 15);

            var label = new Label { FontSize = 15, Content = _vertexIndex };
            Canvas.SetLeft(label, xCoordinate - 10);
            Canvas.SetTop(label, yCoordinate - 15);

            VerticesCoordinates.Add(new Point(xCoordinate, yCoordinate));

            CnvGraph.Children.Add(ellipse);
            CnvGraph.Children.Add(label);

            VisualVertices.Add(new VisualVertex(ellipse, label));

            Vertices.Add($"{_vertexIndex}:{name.Text}");
        }

        private void CreateEdge(ComboBox fromVertex, ComboBox toVertex)
        {
            Point fromPoint = VerticesCoordinates[fromVertex.SelectedIndex];
            Point toPoint = VerticesCoordinates[toVertex.SelectedIndex];

            var pointsCollection = new PointCollection { fromPoint, toPoint };

            var label = new Label { FontSize = 15, Content = TxtWeight.Text };
            Canvas.SetLeft(label, MidPoint(fromPoint, toPoint).X);
            Canvas.SetTop(label, MidPoint(fromPoint, toPoint).Y);

            var edge = new Polyline { Points = pointsCollection };

            CnvGraph.Children.Add(edge);
            CnvGraph.Children.Add(label);

            VisualEdge.Add(new VisualEdges(edge, label));
        }

        private static Point MidPoint(Point fromVertex, Point toVertex)
        {
            double x = (fromVertex.X + toVertex.X) / 2;
            double y = (fromVertex.Y + toVertex.Y) / 2;

            if (fromVertex.Y < toVertex.Y)
            {
                if (fromVertex.X < toVertex.X)
                {
                    y -= 30;
                }
                else if (fromVertex.X > toVertex.X)
                {
                    x -= 25;
                    y -= 30;
                }
            }
            else if (fromVertex.Y > toVertex.Y)
            {
                if (fromVertex.X < toVertex.X)
                {
                    x -= 25;
                    y -= 30;
                }
                else if (fromVertex.X > toVertex.X)
                {
                    y -= 30;
                }
            }

            return new Point(x, y);
        }

        private void RefreshItems()
        {
            LsBoxVertices.Items.Refresh();

            CmbFromVertex.Items.Refresh();
            CmbToVertex.Items.Refresh();

            CmbShortestFrom.Items.Refresh();
            CmbShortestTo.Items.Refresh();

            CmbEditVertex.Items.Refresh();
        }


        private void ClearHighlight()
        {
            foreach (var visualVertex in VisualVertices) visualVertex.Ellipse.Stroke = Brushes.Black;
            foreach (var visualVertex in VisualVertices) visualVertex.Ellipse.Fill = Brushes.White;
        }

        #endregion

        #region Navigation and Extra Elements

        private void HighlightSelectedVertices(object sender, SelectionChangedEventArgs e)
        {
            if (LsBoxVertices.SelectedIndex <= -1)
                return;

            BtnDelete.IsEnabled = true;

            ShortestPathLine.Visibility = Collapsed;

            ClearHighlight();

            VisualVertices[LsBoxVertices.SelectedIndex].Ellipse.Stroke = Brushes.Blue;
            VisualVertices[LsBoxVertices.SelectedIndex].Ellipse.Fill = Brushes.DeepSkyBlue;
        }

        private void ReturnButton(object sender, RoutedEventArgs e)
        {
            TabOption.SelectedIndex = 0;

            ShortestPathLine.Visibility = Collapsed;
            TempEllipse.Visibility = Collapsed;

        }

        private void BtnEdit_OnClick(object sender, RoutedEventArgs e)
        {
            TabOption.SelectedIndex = 2;

            ShortestPathLine.Visibility = Collapsed;
            TempEllipse.Visibility = Collapsed;
        }

        #endregion


        
    }
}
