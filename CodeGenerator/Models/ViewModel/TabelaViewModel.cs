using Microsoft.AspNetCore.Mvc;

namespace CodeGenerator.Models
{
    public class TabelaViewModel
    {
        public TabelaViewModel()
        {
        }

        public TabelaViewModel(List<Tabela>? tabelas, IFormFile? arquivo, string name)
        {
            Tabelas = tabelas;
            Arquivo = arquivo;
            Namespace = name;
        }
        public string Namespace { get; set; }

        public List<Tabela>? Tabelas { get; set; }

        public IFormFile? Arquivo { get; set; }
    }
}
