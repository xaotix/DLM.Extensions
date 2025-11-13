using Conexoes;
using System.Windows.Controls;
using System.Windows.Media;

namespace DLM.Extensions
{
    public static class Extensoes_TreeView
    {
        public static TreeViewItem AddNo(this TreeView treeView, string text, object objeto, ImageSource imagem = null)
        {
            var node = Utilz.Forms.AddTreeview(text, objeto, imagem);
            node.ItemsSource = null;
            treeView.Items.Add(node);

            return node;
        }
        public static TreeViewItem AddNo(this TreeViewItem treeViewItem, string text, object objeto, ImageSource imagem = null)
        {
            var node = Utilz.Forms.AddTreeview(text, objeto, imagem);

            treeViewItem.Items.Add(node);

            return node;
        }
    }
}
