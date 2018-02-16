using WebAPI_SetaDigital.Model;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebAPI_SetaDigital.Controllers
{
    /*Aqui estamos utilizando um padrão de projeto chamado Singleton.
     * Normalmente utilizamos o padrão de projeto singleton para alocar apenas uma instancia de nossa classe.
     * Assim garantimos um ponto de acesso global ao acesso ao nosso banco.
     * Referencia: https://msdn.microsoft.com/en-us/library/ff650316.aspx
     */
    public class Banco
    {
        private static Banco instance;
        private string connstring; //string de conexão do banco

        //todo (1) Modificar as conexões do banco de dados para sua rede local. - OK
        private readonly string host = "10.1.0.43";
        private readonly string port = "5432";
        private readonly string user = "postgres";
        private readonly string pass = "seta";
        private readonly string database = "Bini Backup";


        private Banco()
        {
            connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};", host, port, user, pass, database);
        }

        public static Banco Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Banco();
                }
                return instance;
            }
        }

        //Novos métodos/////////////////////////////////////////////////////////////////
        public List<ContagemClientes> contagemClientesUFs (string pais) { //localhost:5000/api/geoseta/BR
            List<ContagemClientes> listEstados = new List<ContagemClientes> ();//Cria uma lista de ContagemClientes para posteriormente passar como JSON para o FrontEnd
            ContagemClientes estado = new ContagemClientes ();
            int contador=0;
            try {
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                //todo Colocar o Pais
                string sql = String.Format ("select UF, count(codigo) from pessoas where CEP != '' and UF != 'EX' and cliente = 't' group by UF");
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                while (dRead.Read ()) {
                    contador++;
                    estado = new ContagemClientes {
                        nome = "SETA."+pais+"."+ dRead[0].ToString ().Trim (),
                        contagem = Convert.ToInt16(dRead[1].ToString ().Trim ())
                    };
                    Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                    Console.WriteLine("Nome: "+estado.nome+"\nContagem"+estado.contagem);
                    listEstados.Add (estado);

                }

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listEstados;
        }
        public List<ContagemClientes> contagemClientesCidades (string pais, string estado) { //localhost:5000/api/geoseta/BR
            List<ContagemClientes> listCidades = new List<ContagemClientes> ();//Cria uma lista de ContagemClientes para posteriormente passar como JSON para o FrontEnd
            ContagemClientes cidade = new ContagemClientes ();
            int contador=0;
            try {
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                //todo Colocar o pais
                string sql = String.Format ("select cid.descricao, count(*) from pessoas as p inner join cepcidades as cid on p.codcidade = cid.codigo where cid.uf = '{0}' group by cid.descricao",estado);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                while (dRead.Read ()) {
                    contador++;
                    cidade = new ContagemClientes {
                        nome = "SETA."+pais+"."+estado+"."+dRead[0].ToString ().Trim (),
                        contagem = Convert.ToInt16(dRead[1].ToString ().Trim ())
                    };
                    Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                    Console.WriteLine("Nome: "+cidade.nome+"\nContagem"+cidade.contagem);
                    listCidades.Add (cidade);

                }

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listCidades;
        }
        public List<ContagemClientes> contagemClientesBairros (string pais, string estado, string cidade) { //localhost:5000/api/geoseta/BR
            List<ContagemClientes> listBairros = new List<ContagemClientes> ();//Cria uma lista de ContagemClientes para posteriormente passar como JSON para o FrontEnd
            ContagemClientes bairro = new ContagemClientes ();
            int contador=0;
            try {
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                // Colocar o Pais            
                string sql = String.Format ("select bai.descricao, count(p.codigo) from pessoas as p inner join cep on p.cep = cep.codigo inner join cepbairros as bai on bai.codigo = cep.bairro inner join cepcidades as cid on cep.cidade = cid.codigo where p.codcidade = cep.cidade and cid.descricao = '{0}' and cid.uf = '{1}' group by bai.descricao",cidade,estado);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                while (dRead.Read ()) {
                    contador++;
                    bairro = new ContagemClientes {
                        nome = "SETA."+pais+"."+estado+"."+cidade+"."+dRead[0].ToString ().Trim (),
                        contagem = Convert.ToInt16(dRead[1].ToString ().Trim ())
                    };
                    Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                    Console.WriteLine("Nome: "+bairro.nome+"\nContagem"+bairro.contagem);
                    listBairros.Add (bairro);

                }

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listBairros;
        }
        public List<TotalGasto> gastoEstados (string pais) { //localhost:5000/api/geoseta/BR
            List<TotalGasto> listEstados = new List<TotalGasto> ();//Cria uma lista de ContagemClientes para posteriormente passar como JSON para o FrontEnd
            TotalGasto estado = new TotalGasto ();
            int contador=0;
            try {
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                // Colocar o Pais e a query
                // Decidir se vai fazer o gasto médio por bairro ou se vai fazer o gasto total, se for o médio é só pegar o total que vem e dividir pela quantidade de pessoas          
                string sql = String.Format ("select p.UF, sum(v.total), count(v.codigo) from pessoas as p join vendas as v on v.cliente = p.codigo join movimento as m on SUBSTRING(m.auxiliar, 3, 8)::Char(8) = v.codigo and m.operacao = 'VE' where p.uf != '' group by p.uf order by p.uf ");
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                while (dRead.Read ()) {
                    contador++;
                    estado = new TotalGasto {
                        nome = "SETA."+pais+"."+dRead[0].ToString ().Trim (),
                        valor = Convert.ToDouble(dRead[1].ToString ().Trim ())
                    };
                    Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                    Console.WriteLine("Nome: "+estado.nome+"\nValor: "+estado.valor);
                    listEstados.Add (estado);

                }

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listEstados;
        }
        public List<TotalGasto> gastoCidades (string pais, string estado) { //localhost:5000/api/geoseta/BR
            List<TotalGasto> listCidades = new List<TotalGasto> ();//Cria uma lista de ContagemClientes para posteriormente passar como JSON para o FrontEnd
            TotalGasto cidade = new TotalGasto ();
            int contador=0;
            try {
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                // Colocar o Pais
                // Decidir se vai fazer o gasto médio por bairro ou se vai fazer o gasto total, se for o médio é só pegar o total que vem e dividir pela quantidade de pessoas          
                string sql = String.Format ("select cid.descricao, sum(v.total), count(v.codigo) from pessoas as p inner join cepcidades as cid on p.codcidade = cid.codigo join vendas as v on v.cliente = p.codigo join movimento as m on SUBSTRING(m.auxiliar, 3, 8)::Char(8) = v.codigo and m.operacao = 'VE' where cid.uf = '{0}' group by cid.descricao order by cid.descricao",estado);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                while (dRead.Read ()) {
                    contador++;
                    cidade = new TotalGasto {
                        nome = "SETA."+pais+"."+estado+"."+dRead[0].ToString ().Trim (),
                        valor = Convert.ToDouble(dRead[1].ToString ().Trim ())
                    };
                    Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                    Console.WriteLine("Nome: "+cidade.nome+"\nValor: "+cidade.valor);
                    listCidades.Add (cidade);

                }

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listCidades;
        }
        public List<TotalGasto> gastoBairros (string pais, string estado, string cidade) { //localhost:5000/api/geoseta/BR
            List<TotalGasto> listBairros = new List<TotalGasto> ();//Cria uma lista de ContagemClientes para posteriormente passar como JSON para o FrontEnd
            TotalGasto bairro = new TotalGasto ();
            int contador=0;
            try {
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                // Colocar o Pais
                // Decidir se vai fazer o gasto médio por bairro ou se vai fazer o gasto total, se for o médio é só pegar o total que vem e dividir pela quantidade de pessoas          
                string sql = String.Format ("select bai.descricao, sum(v.total), count(v.codigo) from pessoas as p join cep on p.cep = cep.codigo join cepbairros as bai on cep.bairro = bai.codigo join cepcidades as cid on cep.cidade = cid.codigo join vendas as v on v.cliente = p.codigo join movimento as m on SUBSTRING(m.auxiliar, 3, 8)::Char(8) = v.codigo and m.operacao = 'VE' where p.codcidade = cep.cidade and p.cep != '' and cid.descricao = '{0}' and cid.uf = '{1}' group by bai.descricao",cidade,estado);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                while (dRead.Read ()) {
                    contador++;
                    bairro = new TotalGasto {
                        nome = "SETA."+pais+"."+estado+"."+cidade+"."+dRead[0].ToString ().Trim (),
                        valor = Convert.ToDouble(dRead[1].ToString ().Trim ())
                    };
                    Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                    Console.WriteLine("Nome: "+bairro.nome+"\nValor: "+bairro.valor);
                    listBairros.Add (bairro);

                }

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listBairros;
        }
        //Fim dos novos métodos/////////////////////////////////////////////////////////

        public Pessoa BuscarPessoa(string codigo)
        {
            Pessoa pessoa = new Pessoa();//inicialização de Objeto normal (estilo Java)
            try
            {
                //Abro conexão com o banco!
                NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();

                //Aqui mostro como passar parametro :D
                string sql = String.Format("SELECT codigo, nome, cpfcnpj, status FROM pessoas WHERE codigo = '{0}'", codigo);
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader();

                if (dRead.Read())
                {
                    //inicialização de Objeto que deve ser usada (Padrão C#)
                    pessoa = new Pessoa
                    {
                        Codigo = dRead[0].ToString().Trim(),
                        Nome = dRead[1].ToString().Trim(),
                        Cpf = dRead[2].ToString().Trim(),
                        Status = dRead[3].ToString().Trim()
                    };
                }

                //Fecho Conexão com o banco!
                conn.Close();
            }
            catch (Exception msg)
            {
                //Deu pau!!! O que pode ter acontecido?!?!
                Console.WriteLine(msg.ToString());
            }
            return pessoa;
        }

        //todo (3) criem um metodo que Salva o cadastro de um usuário no banco do seta.
        public string CadastrarPessoa(Pessoa pessoa)
        {
            try
            {
                //Abro conexão com o banco!
                NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();

                //Aqui mostro como passar parametro :D
                string sql = String.Format("INSERT INTO pessoas (nome, pessoa, cep, endereco, bairro, cidade, uf, cpfcnpj, estadocivil, status, sexo, cadastro, cliente) VALUES ('{0}',	'{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}','{10}', '{11}', '{12}') RETURNING codigo", pessoa.Nome, pessoa.Tipo, pessoa.Cep, pessoa.Endereco, pessoa.Bairro, pessoa.Cidade, pessoa.Uf, pessoa.Cpf, pessoa.Estadocivil, pessoa.Status, pessoa.Sexo, pessoa.Cadastro, pessoa.Cliente);
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader();

                if (dRead.Read())
                {
                    //inicialização de Objeto que deve ser usada (Padrão C#)
                    pessoa.Codigo = dRead[0].ToString().Trim();
                }

                //Fecho Conexão com o banco!
                conn.Close();
            }
            catch (Exception msg)
            {
                //Deu pau!!! O que pode ter acontecido?!?!
                Console.WriteLine(msg.ToString());
            }
            return pessoa.Codigo;
        }

        public string CadastrarPreVenda(Venda prevenda)
        {
            try
            {
                //Abro conexão com o banco!
                NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();

                //Aqui mostro como passar parametro :D
                string sql = String.Format("INSERT INTO vendas (empresa, tipo, cliente, vendedor, condicoes, status, data, hora, itens, subtotal, total, pecas, clientestatus, prevenda, cpfcnpj) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}','{5}', '{6}', '{7}', {8}, {9}, {10}, {11}, '{12}', '{13}', '{14}') RETURNING codigo", 
                                                        prevenda.Empresa, prevenda.Tipo, prevenda.Cliente, prevenda.Vendedor, prevenda.Condicoes, prevenda.Status,prevenda.Data,prevenda.Hora,prevenda.Itens,prevenda.SubTotal.ToString().Replace(",", "."),prevenda.Total.ToString().Replace(",", "."),prevenda.Itens,prevenda.ClienteStatus, prevenda.PreVenda, prevenda.Cpf);
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader();

                if (dRead.Read())
                {
                    //inicialização de Objeto que deve ser usada (Padrão C#)
                    prevenda.Codigo = dRead[0].ToString().Trim();
                }

                //Fecho Conexão com o banco!
                conn.Close();
            }
            catch (Exception msg)
            {
                //Deu pau!!! O que pode ter acontecido?!?!
                Console.WriteLine(msg.ToString());
            }
            return prevenda.Codigo;
        }

        public string CadastrarMovimentoPreVenda(Movimento movimento)
        {
            try
            {
                //Abro conexão com o banco!
                NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();
                
                //Aqui mostro como passar parametro :D
                string sql = String.Format("INSERT INTO movimento (auxiliar, operacao, movimento, data, empresa, produto, quantidade, unitario, total, vendedorm, custo) VALUES ('{0}','{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', {10}) RETURNING codigo", movimento.Auxiliar,movimento.Op, movimento.Mov, movimento.Data, movimento.Empresa, movimento.CodProduto, movimento.Qtd, movimento.Unitario.ToString().Replace(",", ".") ,movimento.Total.ToString().Replace(",", "."), movimento.Vendedor, movimento.Custo.ToString().Replace(",", "."));
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader();

                if (dRead.Read())
                {
                    //inicialização de Objeto que deve ser usada (Padrão C#)
                    movimento.Codigo = dRead[0].ToString().Trim();
                }

                //Fecho Conexão com o banco!
                conn.Close();
            }
            catch (Exception msg)
            {
                //Deu pau!!! O que pode ter acontecido?!?!
                Console.WriteLine(msg.ToString());
            }
            return movimento.Codigo;
        }
         public decimal BuscarInfoProduto(string codigo, string opcao)
        {
            Pessoa pessoa = new Pessoa();//inicialização de Objeto normal (estilo Java)
            decimal preco=0;
            try
            {
                //Abro conexão com o banco!
                NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();

                //Aqui mostro como passar parametro :D
                string sql = String.Format("SELECT {0} FROM produtos WHERE codigo = '{1}'", opcao, codigo);
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader();

                if (dRead.Read())
                {
                    //inicialização de Objeto que deve ser usada (Padrão C#)
                        preco = dRead.GetDecimal(0);
                }

                //Fecho Conexão com o banco!
                conn.Close();
            }
            catch (Exception msg)
            {
                //Deu pau!!! O que pode ter acontecido?!?!
                Console.WriteLine(msg.ToString());
            }
            return preco;
        }


        public void AlterarPessoa(Pessoa pessoa)
        {
            
            try
            {
                //Abro conexão com o banco!
                NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();

                //Aqui mostro como passar parametro :D
                string sql = String.Format("UPDATE pessoas SET status = '{0}' WHERE codigo = '{1}'",pessoa.Status, pessoa.Codigo);
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader();

                //Fecho Conexão com o banco!
                conn.Close();
            }
            catch (Exception msg)
            {
                //Deu pau!!! O que pode ter acontecido?!?!
                Console.WriteLine(msg.ToString());
            }
        }

    }
}
