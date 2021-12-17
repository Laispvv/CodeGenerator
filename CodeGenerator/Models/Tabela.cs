namespace CodeGenerator.Models
{
    public class Tabela
    {
        public Tabela()
        {
        }

        public Tabela(string nome, bool geraArquivo)
        {
            Nome = nome;
            GeraArquivo = geraArquivo;
        }

        public string Nome { get; set; }
        public bool GeraArquivo { get; set; }

    }
}
