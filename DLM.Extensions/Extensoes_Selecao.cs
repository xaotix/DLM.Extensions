using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace Conexoes
{
    public static class Extensoes_Selecao
    {
        public static T ListaSelecionar<T>(this List<T> Objetos, T Selecao, string titulo = "Selecione")
        {
            if (Objetos.Count == 0 | Objetos == null)
            {
                return (T)Convert.ChangeType(null, typeof(T));
            }
            var selecionar = new JanelaSelecionar(false, Selecao);
            selecionar.Title = titulo;
            selecionar._lista.ItemsSource = Objetos;
            selecionar.ShowDialog();

            if (selecionar.DialogResult.HasValue && selecionar.DialogResult.Value)
            {
                if (selecionar._lista.SelectedItem != null)
                {
                    return (T)Convert.ChangeType(selecionar._lista.SelectedItem, typeof(T));
                }
            }
            try
            {
                return (T)Convert.ChangeType(null, typeof(T));
            }
            catch (Exception)
            {
                return Objetos[0];
            }
        }
        public static T ListaSelecionar<T>(this List<T> Objetos, string titulo = "Selecione")
        {

            if (Objetos.Count == 0 | Objetos == null)
            {
                return (T)Convert.ChangeType(null, typeof(T));
            }
            var selecionar = new JanelaSelecionar(false);
            selecionar.Title = titulo;
            selecionar._lista.ItemsSource = Objetos;
            selecionar.ShowDialog();

            if (selecionar.DialogResult.HasValue && selecionar.DialogResult.Value)
            {
                if (selecionar._lista.SelectedItem != null)
                {
                    return (T)Convert.ChangeType(selecionar._lista.SelectedItem, typeof(T));
                }
            }
            try
            {
                return (T)Convert.ChangeType(null, typeof(T));
            }
            catch (Exception)
            {
                return Objetos[0];
            }
        }
        public static Cor_RAL ListaSelecionar(this List<Cor_RAL> Objetos)
        {
            if (Objetos.Count == 0)
            {
                return null;
            }
            var selecionar = new Janelas.Seleciona_Cor(Objetos);

            selecionar.Title = "Selecione";
            selecionar.Lista.ItemsSource = Objetos;
            selecionar.Lista.Selecionar(Buff.Cor_RAL);

            selecionar.ShowDialog();
            if (selecionar.DialogResult.HasValue && selecionar.DialogResult.Value)
            {
                if (selecionar.Lista.SelectedItem != null)
                {
                    Buff.Cor_RAL = selecionar.Lista.SelectedItem as Cor_RAL;
                    return Buff.Cor_RAL;
                }
            }
            return null;

        }
        public static Bobina ListaSelecionar(this List<Bobina> Objetos)
        {
            if (Objetos.Count == 0)
            {
                return null;
            }
            var selecionar = new Janelas.Selecionar_Bobina(Objetos);
            selecionar.Title = "Bobinas";
            selecionar.Lista.ItemsSource = Objetos;
            selecionar.Lista.Selecionar(Buff.Bobina);
            selecionar.ShowDialog();
            if (selecionar.DialogResult.HasValue && selecionar.DialogResult.Value)
            {
                if (selecionar.Lista.SelectedItem != null)
                {
                    Buff.Bobina = selecionar.Lista.SelectedItem as Bobina;
                    return Buff.Bobina;
                }
            }
            return null;

        }


        public static List<T> ListaSelecionarVarios<T>(this List<T> Objetos, bool selecionar_tudo, string titulo = "Selecione")
        {
            if (Objetos == null)
            {
                return new List<T>();
            }
            if (Objetos.Count == 0)
            {
                return new List<T>();
            }
            if (selecionar_tudo)
            {
                return ListaSelecionarVarios(new List<T>(), Objetos, titulo);
            }
            else
            {
                return ListaSelecionarVarios(Objetos, new List<T>(), titulo);
            }
        }
        public static List<T> ListaSelecionarVarios<T>(this List<T> Objetos, List<T> Selecionar = null, string titulo = "Selecione")
        {
            return ListaSelecionarVarios(Objetos, false, true, titulo, null, Selecionar);
        }
        private static List<T> ListaSelecionarVarios<T>(this List<T> Objetos, bool selecionar_tudo, bool duas_colunas, string Titulo, Window owner = null, List<T> Selecionar = null)
        {
            if (!duas_colunas)
            {
                var mm = new JanelaSelecionar(selecionar_tudo);
                mm.Title = Titulo;
                mm._lista.ItemsSource = Objetos;
                mm._lista.SelectionMode = System.Windows.Controls.SelectionMode.Extended;



                if (owner != null)
                {
                    mm.Owner = owner;
                    mm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                mm.ShowDialog();
                if (mm.DialogResult.HasValue && mm.DialogResult.Value)
                {
                    if (mm._lista.SelectedItem != null)
                    {
                        return mm._lista.SelectedItems.Cast<T>().ToList();
                    }
                }
            }
            else
            {
                var selecao = new List<object>();
                if (Selecionar != null)
                {
                    selecao = Selecionar.Cast<object>().ToList();
                }
                var mm = new JanelaAdicionarDuasColunas(Objetos.Cast<object>().ToList(), selecao);
                mm.Title = Titulo;
                if (owner != null)
                {
                    mm.Owner = owner;
                    mm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                else
                {
                    mm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
                mm.ShowDialog();
                if (mm.DialogResult.HasValue && mm.DialogResult.Value)
                {
                    return mm.Selecao.Cast<T>().ToList();
                }
            }
            return new List<T>();
        }


        public static void Selecionar(this System.Windows.Controls.ListView view, object objeto)
        {
            if (objeto == null) { return; }
            try
            {
                view.SelectedItem = objeto;
                view.UpdateLayout();
                if (view.SelectedItem != null)
                {
                    view.ScrollIntoView(view.SelectedItem);
                }
            }
            catch (Exception)
            {
            }
        }
        public static void Selecionar(this System.Windows.Controls.DataGrid view, object objeto)
        {
            if (objeto == null) { return; }
            try
            {
                view.SelectedItem = objeto;
                view.UpdateLayout();
                if (view.SelectedItem != null)
                {
                    view.ScrollIntoView(view.SelectedItem);
                }
            }
            catch (Exception)
            {
            }
        }
        public static void SelecionaUltimo(this System.Windows.Controls.DataGrid view)
        {
            view.Selecionar(view.Items[view.Items.Count - 1]);
        }
    }
}
