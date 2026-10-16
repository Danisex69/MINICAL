using System.Globalization;

namespace MINICAL;

public partial class MainPage : ContentPage
{
    string expresion = "0";         
    bool acabaDeCalcular = false;    

    public MainPage()
    {
        InitializeComponent();
        MostrarPantalla();
    }


    void Numero_Clicked(object sender, EventArgs e)
    {
        string texto = ((Button)sender).Text;
        char c = texto[0];

        
        if (!char.IsDigit(c) && c != '.')
        {
            Mensaje.Text = "Solo se permiten números";
            return;
        }

        
        if (acabaDeCalcular)
        {
            expresion = "0";
            acabaDeCalcular = false;
        }

        if (expresion.Length >= 25) return;
        if (expresion.EndsWith("%")) return;

        string numeroActual = ObtenerNumeroActual();

        if (c == '.')
        {
           
            if (numeroActual.Contains('.'))
            {
                Mensaje.Text = "Solo se permite un punto por número";
                return;
            }

            if (numeroActual == "") expresion += "0.";
            else expresion += ".";
        }
        else
        {
            if (numeroActual == "0")
                expresion = expresion.Substring(0, expresion.Length - 1) + c;
            else
                expresion += c;
        }

        Mensaje.Text = "";
        MostrarPantalla();
    }


    void Operador_Clicked(object sender, EventArgs e)
    {
        char op = ((Button)sender).Text[0];
        if (op == '\u2212') op = '-';

        acabaDeCalcular = false;

        char ultimo = expresion[expresion.Length - 1];

        if (EsOperador(ultimo))
            expresion = expresion.Substring(0, expresion.Length - 1) + op;
        else if (ultimo == '.')
            expresion += "0" + op;
        else
            expresion += op;

        Mensaje.Text = "";
        MostrarPantalla();
    }


    void Porcentaje_Clicked(object sender, EventArgs e)
    {
        char ultimo = expresion[expresion.Length - 1];

        if (!char.IsDigit(ultimo))
        {
            Mensaje.Text = "El % va después de un número";
            return;
        }

        acabaDeCalcular = false;
        expresion += "%";
        Mensaje.Text = "";
        MostrarPantalla();
    }


    void Borrar_Clicked(object sender, EventArgs e)
    {
        acabaDeCalcular = false;
        expresion = expresion.Substring(0, expresion.Length - 1);

        if (expresion == "" || expresion == "-") expresion = "0";

        Mensaje.Text = "";
        MostrarPantalla();
    }

    void Limpiar_Clicked(object sender, EventArgs e)
    {
        acabaDeCalcular = false;
        expresion = "0";
        Mensaje.Text = "";
        MostrarPantalla();
    }


    void Igual_Clicked(object sender, EventArgs e)
    {

        string operacion = expresion.TrimEnd('+', '-', '×', '÷');
        if (operacion == "") return;

        try
        {
            double resultado = Evaluar(operacion);

            if (double.IsInfinity(resultado) || double.IsNaN(resultado) || Math.Abs(resultado) >= 1e15)
            {
                Mensaje.Text = "El resultado es demasiado grande";
                return;
            }

            resultado = Math.Round(resultado, 10);
            string texto = resultado.ToString("0.##########", CultureInfo.InvariantCulture);
            if (texto == "-0") texto = "0";

            Mensaje.Text = operacion + " =";
            expresion = texto;
            acabaDeCalcular = true;
            MostrarPantalla();
        }
        catch (DivideByZeroException)
        {
            
            Mensaje.Text = "No se puede dividir entre cero";
        }
    }


    double Evaluar(string texto)
    {
        List<double> numeros = new List<double>();
        List<char> operadores = new List<char>();
        string actual = "";
        bool negativo = false;

        
        if (texto.StartsWith("-"))
        {
            negativo = true;
            texto = texto.Substring(1);
        }

        
        foreach (char c in texto)
        {
            if (char.IsDigit(c) || c == '.')
            {
                actual += c;
            }
            else if (c == '%')
            {
                
                double valor = double.Parse(actual, CultureInfo.InvariantCulture) / 100;
                actual = valor.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                numeros.Add(double.Parse(actual, CultureInfo.InvariantCulture));
                operadores.Add(c);
                actual = "";
            }
        }
        numeros.Add(double.Parse(actual, CultureInfo.InvariantCulture));

        if (negativo) numeros[0] = -numeros[0];

        /
        int i = 0;
        while (i < operadores.Count)
        {
            if (operadores[i] == '×' || operadores[i] == '÷')
            {
                if (operadores[i] == '÷' && numeros[i + 1] == 0)
                    throw new DivideByZeroException();

                if (operadores[i] == '×')
                    numeros[i] = numeros[i] * numeros[i + 1];
                else
                    numeros[i] = numeros[i] / numeros[i + 1];

                numeros.RemoveAt(i + 1);
                operadores.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }

        
        double resultado = numeros[0];
        for (i = 0; i < operadores.Count; i++)
        {
            if (operadores[i] == '+') resultado += numeros[i + 1];
            else resultado -= numeros[i + 1];
        }

        return resultado;
    }


    bool EsOperador(char c)
    {
        return c == '+' || c == '-' || c == '×' || c == '÷';
    }


    string ObtenerNumeroActual()
    {
        int i = expresion.Length;
        while (i > 0 && (char.IsDigit(expresion[i - 1]) || expresion[i - 1] == '.'))
            i--;
        return expresion.Substring(i);
    }

    void MostrarPantalla()
    {
        Pantalla.Text = expresion;


        if (expresion.Length <= 7) Pantalla.FontSize = 80;
        else if (expresion.Length <= 10) Pantalla.FontSize = 56;
        else if (expresion.Length <= 14) Pantalla.FontSize = 40;
        else Pantalla.FontSize = 28;
    }
}

