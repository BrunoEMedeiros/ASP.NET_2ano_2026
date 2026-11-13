// Trocar pelo nome da pasta do seu projeto
namespace Atividade.Models;

public class Produto(string nome, decimal preco){
    public int? Id {get; set;}  
    public string Nome {get; set;} = nome;
    public decimal Preco {get; set;} = preco;
}


