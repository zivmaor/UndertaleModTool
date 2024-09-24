using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace UndertaleModTool
{
    /// <summary>
    /// Interaction logic for UndertaleGlobalInitEditor.xaml
    /// </summary>
    public partial class UndertaleGlobalInitEditor : DataUserControl
    {
        public const double MinScriptsGridHeight = 100;

        public UndertaleGlobalInitEditor()
        {
            InitializeComponent();
        }

        private void UndertaleGlobalInitEditor_Loaded(object sender, RoutedEventArgs e)
        {
            // Calculates the min height of the Editor, so it won't resize any smaller.
            // At min height all fields and at least a few lines of scripts grid should be displayed.
            // Must be done at runtime since the size of fields is unknown until they are rendered.
            this.MinHeight = FieldsGrid.ActualHeight + MinScriptsGridHeight;
        }
    }
}
