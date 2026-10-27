
using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace MauiApp1;

public partial class MainPage : ContentPage
{
    private double parcial1;
    private double parcial2;
    private double notaFinal;

    private string estado = "SIN CALCULAR";
    private bool calculado = false;

    public MainPage()
    {
        InitializeComponent();

        EstudiantePicker.SelectedIndex = 0;
        FechaEvaluacion.Date = DateTime.Today;
    }

    private async void MostrarError(string mensaje)
    {
        await DisplayAlertAsync(
            "Error de validación",
            mensaje,
            "Aceptar"
        );
    }

    private bool ValidarNota(
        Entry entrada,
        string nombre,
        out double nota)
    {
        nota = 0;

        string texto = entrada.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(texto))
        {
            MostrarError("Ingresa la nota de " + nombre);
            return false;
        }

        texto = texto.Replace(",", ".");

        bool correcto = double.TryParse(
            texto,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out nota
        );

        if (!correcto || double.IsNaN(nota) ||
            double.IsInfinity(nota))
        {
            MostrarError(nombre + " debe ser un número válido.");
            return false;
        }

        if (nota < 0 || nota > 10)
        {
            MostrarError(nombre + " debe estar entre 0 y 10.");
            return false;
        }

        return true;
    }

    private async void Calcular_Clicked(
        object? sender,
        EventArgs e)
    {
        if (EstudiantePicker.SelectedIndex == -1)
        {
            await DisplayAlertAsync(
                "Atención",
                "Selecciona un estudiante.",
                "Aceptar"
            );
            return;
        }

        if (!ValidarNota(
            Seguimiento1Entry,
            "Seguimiento 1",
            out double s1))
            return;

        if (!ValidarNota(
            Examen1Entry,
            "Examen 1",
            out double e1))
            return;

        if (!ValidarNota(
            Seguimiento2Entry,
            "Seguimiento 2",
            out double s2))
            return;

        if (!ValidarNota(
            Examen2Entry,
            "Examen 2",
            out double e2))
            return;

        parcial1 = (s1 * 0.30) + (e1 * 0.20);
        parcial2 = (s2 * 0.30) + (e2 * 0.20);

        notaFinal = parcial1 + parcial2;

        estado = notaFinal >= 7
            ? "APROBADO"
            : "REPRUEBA";

        calculado = true;

        Parcial1Label.Text = $"{parcial1:F2} / 5.00";
        Parcial2Label.Text = $"{parcial2:F2} / 5.00";

        NotaFinalLabel.Text = $"{notaFinal:F2} / 10.00";

        double porcentaje = notaFinal * 10;

        PorcentajeLabel.Text =
            $"{porcentaje:F0}% de rendimiento";

        ProgresoNota.Progress = notaFinal / 10;

        EstadoLabel.Text = estado;

        if (notaFinal >= 7)
        {
            EstadoBorder.BackgroundColor =
                Color.FromArgb("#174A37");

            EstadoLabel.TextColor =
                Color.FromArgb("#76E9AD");

            MensajeEstadoLabel.Text =
                "El estudiante aprobó correctamente.";
        }
        else
        {
            EstadoBorder.BackgroundColor =
                Color.FromArgb("#552835");

            EstadoLabel.TextColor =
                Color.FromArgb("#FF8F9C");

            MensajeEstadoLabel.Text =
                "El estudiante no alcanzó la nota mínima.";
        }

        AgregarHistorial();

        await ScrollPrincipal.ScrollToAsync(
            PanelResultados,
            ScrollToPosition.End,
            true
        );
    }

    private string ObtenerFecha()
    {
        DateTime fecha = FechaEvaluacion.Date ?? DateTime.Today;
        return fecha.ToString("dd/MM/yyyy");
    }

    private void AgregarHistorial()
    {
        HistorialVacioLabel.IsVisible = false;

        string estudiante =
            EstudiantePicker.SelectedItem?.ToString() ?? "";

        Grid fila = new Grid
        {
            ColumnSpacing = 8,
            Padding = new Thickness(10, 12),
            BackgroundColor = Color.FromArgb("#1A3455")
        };

        fila.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(1.4, GridUnitType.Star)
            });

        fila.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(1.2, GridUnitType.Star)
            });

        fila.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = GridLength.Star
            });

        fila.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = GridLength.Star
            });

        fila.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = GridLength.Star
            });

        fila.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(1.2, GridUnitType.Star)
            });

        AgregarTexto(fila, estudiante, 0);
        AgregarTexto(fila, ObtenerFecha(), 1);
        AgregarTexto(fila, parcial1.ToString("F2"), 2);
        AgregarTexto(fila, parcial2.ToString("F2"), 3);
        AgregarTexto(fila, notaFinal.ToString("F2"), 4);

        Color colorEstado = notaFinal >= 7
            ? Color.FromArgb("#76E9AD")
            : Color.FromArgb("#FF8F9C");

        AgregarTexto(fila, estado, 5, colorEstado);

        HistorialContenedor.Children.Insert(0, fila);
    }

    private void AgregarTexto(
        Grid fila,
        string texto,
        int columna,
        Color? color = null)
    {
        Label etiqueta = new Label
        {
            Text = texto,
            TextColor = color ?? Colors.White,
            FontSize = 12,
            VerticalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        };

        Grid.SetColumn(etiqueta, columna);
        fila.Children.Add(etiqueta);
    }

    private async void Reporte_Clicked(
        object? sender,
        EventArgs e)
    {
        if (!calculado)
        {
            await DisplayAlertAsync(
                "Sin resultados",
                "Primero calcula las calificaciones.",
                "Aceptar"
            );
            return;
        }

        string estudiante =
            EstudiantePicker.SelectedItem?.ToString() ?? "";

        string reporte =
            "INSTITUTO SUDAMERICANO\n" +
            "REPORTE ACADÉMICO\n\n" +
            $"Estudiante: {estudiante}\n" +
            $"Fecha: {ObtenerFecha()}\n\n" +
            $"Primer parcial: {parcial1:F2} / 5.00\n" +
            $"Segundo parcial: {parcial2:F2} / 5.00\n\n" +
            $"NOTA FINAL: {notaFinal:F2} / 10.00\n" +
            $"ESTADO: {estado}";

        await DisplayAlertAsync(
            "Reporte académico",
            reporte,
            "Cerrar"
        );
    }

    private void Limpiar_Clicked(
        object? sender,
        EventArgs e)
    {
        Seguimiento1Entry.Text = "";
        Examen1Entry.Text = "";
        Seguimiento2Entry.Text = "";
        Examen2Entry.Text = "";

        Parcial1Label.Text = "0.00 / 5.00";
        Parcial2Label.Text = "0.00 / 5.00";

        NotaFinalLabel.Text = "0.00 / 10.00";
        PorcentajeLabel.Text = "0% de rendimiento";

        ProgresoNota.Progress = 0;

        EstadoLabel.Text = "SIN CALCULAR";
        EstadoLabel.TextColor = Colors.White;

        EstadoBorder.BackgroundColor =
            Color.FromArgb("#293C55");

        MensajeEstadoLabel.Text =
            "Completa las notas y presiona calcular.";

        parcial1 = 0;
        parcial2 = 0;
        notaFinal = 0;

        estado = "SIN CALCULAR";
        calculado = false;

        EstudiantePicker.SelectedIndex = 0;
        FechaEvaluacion.Date = DateTime.Today;
    }
}
