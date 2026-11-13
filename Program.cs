// Chamando as bibliotecas para o arquivo
using Npgsql;
using Dapper;
using Atividade.Models;

// Criando as configurações basicas de uma API Rest
var builder = WebApplication.CreateBuilder(args);

// Configurando o CORS para habilitar todas as trocas
// de informação -- (SOMENTE SALA DE AULA) --
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy =>{
        policy.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Criando uma variavel para armazenar as configurações da conexão com
// o banco de dados
var connectionString = builder.Configuration.GetConnectionString("Default");

// Usando a variavel com as configurações da conexão para, efetivamente,
// estabelecer uma conexão entre a API e o banco de dados
builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString!));

// Cria a variavel app que vai conter todas as configurações feitas
// acima na API
var app = builder.Build();

// Usa as configurações feitas no CORS
app.UseCors();


// Navegador: http://localhost:5080/produtos

// GET - Consulta (Mostrar algo)
app.MapGet("/produtos", async (decimal? preco, NpgsqlDataSource banco) =>{
    // Estabelece uma conexão com meu banco de dados;
    await using var conexao = await banco.OpenConnectionAsync();

    // Criando um SELECT base para executar caso o usuário não pesquisa por preço
    var sql = "SELECT * FROM produtos;";

    // Checando se a pessoa passou o filtro de preço do produto
    if(preco != null){
        sql = $"SELECT * FROM produtos WHERE preco <= @preco;";
    }

    // Executando a consulta
    var produtos = await conexao.QueryAsync(sql, new { preco = preco});
    
    // Retornando os produtos para o usuário
    return Results.Ok(produtos);
});


app.MapGet("/produtos/{nome}", async (string nome, NpgsqlDataSource banco)=>{
    await using var conexao = await banco.OpenConnectionAsync();

    // Usando o Dapper para dar comandos SQL de maneira segura
    var sql = $"SELECT * FROM produtos WHERE nome LIKE @nome";

    // A varivel criada no comando SQL é especificada no new {}
    var produtos = await conexao.QueryAsync(sql, new {nome = $"%{nome}%"});

    return Results.Ok(produtos);
});

app.MapPost("/produtos", 
    async(Produto NovoProduto, NpgsqlDataSource banco)=>{
    await using var conexao = await banco.OpenConnectionAsync();

    var sql = "INSERT INTO produtos(nome, preco) VALUES (@Nome, @Preco)";
    await conexao.QueryAsync(sql, new {Nome = NovoProduto.Nome, Preco = NovoProduto.Preco});

    return Results.Created();

});

// Inicia o serviço da API
app.Run("http://0.0.0.0:5080");