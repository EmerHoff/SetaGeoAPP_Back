using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;
using WebAPI_SetaDigital.Model;

namespace WebAPI_SetaDigital.Controllers {
    /*Aqui estamos utilizando um padrão de projeto chamado Singleton.
     * Normalmente utilizamos o padrão de projeto singleton para alocar apenas uma instancia de nossa classe.
     * Assim garantimos um ponto de acesso global ao acesso ao nosso banco.
     * Referencia: https://msdn.microsoft.com/en-us/library/ff650316.aspx
     */
    public class Banco {
        private static Banco instance;
        private string connstring; //string de conexão do banco

        //todo (1) Modificar as conexões do banco de dados para sua rede local. - OK
        private readonly string host = "10.1.0.43";
        private readonly string port = "5432";
        private readonly string user = "postgres";
        private readonly string pass = "seta";
        private readonly string database = "Bini Backup";

        // private  readonly  string  host  =  "demosd.calcontec.com";
        // private  readonly  string  port  =  "5432";
        // private  readonly  string  user  =  "rede000001";
        // private  readonly  string  pass  =  "37dZ50rF";
        // private  readonly  string  database  =  "teste_a";

        public List<Contagem> Marcas;

        private Banco () {
            connstring = String.Format ("Server={0};Port={1};User Id={2};Password={3};Database={4};", host, port, user, pass, database);
        }

        public static Banco Instance {
            get {
                if (instance == null) {
                    instance = new Banco ();
                }
                return instance;
            }
        }

        //Novos métodos/////////////////////////////////////////////////////////////////
        //Cria uma lista de Contagem com base no pais para posteriormente passar como JSON para o FrontEnd
        //faz a pesquisa com base na UF da pessoa e onde os campos de bairro, cep, cidade nao são vazios para não resultar
        //divergencias com os outros métodos
        public List<Contagem> contagemClientesUFs (string pais) { //localhost:5000/api/geoseta/BR
            List<Contagem> listEstados = new List<Contagem> ();
            Contagem estado = new Contagem ();
            int contador = 0;
            try {
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                //todo Colocar o Pais
                string sql = String.Format ("select UF, count(codigo) from pessoas " +
                                            "where UF != '' and UF != 'EX' and cliente = 't' " +
                                            "and bairro != '' and cidade !='' and cep != '' " +
                                            "and (status = 'A' or status = 'S') group by UF");
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                while (dRead.Read ()) {
                    contador++;
                    estado = new Contagem {
                        nome = "SETA." + pais + "." + dRead[0].ToString ().Trim (),
                        valor = Convert.ToInt32 (dRead[1].ToString ().Trim ())
                    };
                    Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                    Console.WriteLine ("Nome: " + estado.nome + "\nContagem" + estado.valor);
                    listEstados.Add (estado);

                }

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listEstados;
        }
        //Cria uma lista de Contagem que contem a lista de estados e a qtd de clientes para posteriormente passar como JSON para o FrontEnd
        public List<Contagem> contagemClientesCidades (string pais, string estado) { //localhost:5000/api/geoseta/BR
            List<Contagem> listCidades = new List<Contagem> ();
            Contagem cidade = new Contagem ();
            int contador = 0;
            try {
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                //todo Colocar o pais
                string sql = String.Format ("select cid.descricao, count(p.codigo) from pessoas as p " +
                                                "inner join cepcidades as cid on p.codcidade = cid.codigo " +
                                                "where cid.uf = '{0}' and (p.status = 'S' or p.status = 'A') and p.cliente = 't' " +
                                                "and p.bairro != '' and p.cidade !='' and p.cep != '' " +
                                                "group by cid.descricao", estado);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                while (dRead.Read ()) {
                    contador++;
                    cidade = new Contagem {
                        nome = "SETA." + pais + "." + estado + "." + dRead[0].ToString ().Trim (),
                        valor = Convert.ToInt32 (dRead[1].ToString ().Trim ())
                    };
                    Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                    Console.WriteLine ("Nome: " + cidade.nome + "\nContagem" + cidade.valor);
                    listCidades.Add (cidade);

                }

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listCidades;
        }
        //Cria uma lista de Contagem que contém a lista de bairros e a qtd de clientes para posteriormente passar como JSON para o FrontEnd
        public List<Contagem> contagemClientesBairros (string pais, string estado, string cidade) { //localhost:5000/api/geoseta/BR
            List<Contagem> listBairros = new List<Contagem> ();
            Contagem bairro = new Contagem ();
            int contador = 0;
            try {
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                // Colocar o Pais            
                string sql = String.Format ("select bai.descricao, count(p.codigo) from pessoas as p " +
                                                "inner join cep on p.cep = cep.codigo " +
                                                "inner join cepbairros as bai on bai.codigo = cep.bairro " +
                                                "inner join cepcidades as cid on cep.cidade = cid.codigo " +
                                                "where p.codcidade = cep.cidade and cid.descricao = '{0}' " +
                                                "and cid.uf = '{1}' and (p.status = 'S' or p.status = 'A') " +
                                                "and p.bairro != '' and p.cidade !='' and p.cep != '' " +
                                                "and p.cliente = 't' group by bai.descricao", cidade, estado);

                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                while (dRead.Read ()) {
                    contador++;
                    bairro = new Contagem {
                        nome = "SETA." + pais + "." + estado + "." + cidade + "." + dRead[0].ToString ().Trim (),
                        valor = Convert.ToInt32 (dRead[1].ToString ().Trim ())
                    };
                    Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                    Console.WriteLine ("Nome: " + bairro.nome + "\nContagem" + bairro.valor);
                    listBairros.Add (bairro);

                }

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listBairros;
        }
        public List<Contagem> gastoEstados (string pais) { //localhost:5000/api/geoseta/BR
            List<Contagem> listEstados = new List<Contagem> (); //Cria uma lista de Contagem para posteriormente passar como JSON para o FrontEnd
            Contagem estado = new Contagem ();
            int contador = 0;
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
                    estado = new Contagem {
                        nome = "SETA." + pais + "." + dRead[0].ToString ().Trim (),
                        valor = Convert.ToDouble (dRead[1].ToString ().Trim ())
                    };
                    Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                    Console.WriteLine ("Nome: " + estado.nome + "\nValor: " + estado.valor);
                    listEstados.Add (estado);

                }

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listEstados;
        }
        public List<Contagem> gastoCidades (string pais, string estado) { //localhost:5000/api/geoseta/BR
            List<Contagem> listCidades = new List<Contagem> (); //Cria uma lista de Contagem para posteriormente passar como JSON para o FrontEnd
            Contagem cidade = new Contagem ();
            int contador = 0;
            try {
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                // Colocar o Pais
                // Decidir se vai fazer o gasto médio por bairro ou se vai fazer o gasto total, se for o médio é só pegar o total que vem e dividir pela quantidade de pessoas          
                string sql = String.Format ("select cid.descricao, sum(v.total), count(v.codigo) from pessoas as p inner join cepcidades as cid on p.codcidade = cid.codigo join vendas as v on v.cliente = p.codigo join movimento as m on SUBSTRING(m.auxiliar, 3, 8)::Char(8) = v.codigo and m.operacao = 'VE' where cid.uf = '{0}' group by cid.descricao order by cid.descricao", estado);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                while (dRead.Read ()) {
                    contador++;
                    cidade = new Contagem {
                        nome = "SETA." + pais + "." + estado + "." + dRead[0].ToString ().Trim (),
                        valor = Convert.ToDouble (dRead[1].ToString ().Trim ())
                    };
                    Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                    Console.WriteLine ("Nome: " + cidade.nome + "\nValor: " + cidade.valor);
                    listCidades.Add (cidade);

                }

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listCidades;
        }
        public List<Contagem> gastoBairros (string pais, string estado, string cidade) { //localhost:5000/api/geoseta/BR
            List<Contagem> listBairros = new List<Contagem> (); //Cria uma lista de Contagem para posteriormente passar como JSON para o FrontEnd
            Contagem bairro = new Contagem ();
            int contador = 0;
            try {
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                // Colocar o Pais
                // Decidir se vai fazer o gasto médio por bairro ou se vai fazer o gasto total, se for o médio é só pegar o total que vem e dividir pela quantidade de pessoas          
                string sql = String.Format ("select bai.descricao, sum(v.total), count(v.codigo) from pessoas as p join cep on p.cep = cep.codigo join cepbairros as bai on cep.bairro = bai.codigo join cepcidades as cid on cep.cidade = cid.codigo join vendas as v on v.cliente = p.codigo join movimento as m on SUBSTRING(m.auxiliar, 3, 8)::Char(8) = v.codigo and m.operacao = 'VE' where p.codcidade = cep.cidade and p.cep != '' and cid.descricao = '{0}' and cid.uf = '{1}' group by bai.descricao", cidade, estado);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                while (dRead.Read ()) {
                    contador++;
                    bairro = new Contagem {
                        nome = "SETA." + pais + "." + estado + "." + cidade + "." + dRead[0].ToString ().Trim (),
                        valor = Convert.ToDouble (dRead[1].ToString ().Trim ())
                    };
                    Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                    Console.WriteLine ("Nome: " + bairro.nome + "\nValor: " + bairro.valor);
                    listBairros.Add (bairro);

                }

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listBairros;
        }
        public List<ContagemMarca> listMarcasUFs (string pais, int qtd) { //localhost:5000/api/geoseta/BR
            List<ContagemMarca> listEstados = new List<ContagemMarca> (); //Cria uma lista de Contagem para posteriormente passar como JSON para o FrontEnd
            ContagemMarca estado = new ContagemMarca ();
            int contador = 0;
            try {
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                // Colocar o Pais
                // Decidir se vai fazer o gasto médio por bairro ou se vai fazer o gasto total, se for o médio é só pegar o total que vem e dividir pela quantidade de pessoas          
                string sql = String.Format ("select p.UF, mar.descricao, count(m.quantidade) from pessoas as p "+
                "join vendas as v on v.cliente = p.codigo join movimento as m on SUBSTR(m.auxiliar, 3, 8)::Char(8) = v.codigo"+
                " AND m.operacao = 'VE' join produtos as pro on pro.codigo = substr(m.produto, 1, 6)::Char(6)"+
                " join marcas as mar on mar.codigo = pro.marca where p.UF != '' "+
                " and p.UF != 'EX' and p.cliente = 't' group by p.uf, mar.descricao order by p.UF, count desc");
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                string auxiliar = "";
                string anterior = "";
                List<Marca> listaMarcas = new List<Marca> ();
                Marca marca;
                int limit = 0;
                while (dRead.Read ()) {
                    auxiliar = dRead[0].ToString ().Trim ();
                    if (contador == 0) {
                        anterior = auxiliar;
                        marca = new Marca () {
                            Nome = dRead[1].ToString ().Trim (),
                            QtdVendasMarca = Convert.ToInt16 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    }

                    if (contador != 0 && auxiliar == anterior && limit < qtd) {
                        marca = new Marca {
                        Nome = dRead[1].ToString ().Trim (),
                        QtdVendasMarca = Convert.ToInt16 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    } else if (contador != 0 && auxiliar != anterior) {

                        estado = new ContagemMarca {
                        nome = "SETA." + pais + "." + anterior,
                        lstMarca = listaMarcas
                        };
                        Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                        Console.WriteLine ("Nome: " + estado.nome);
                        listEstados.Add (estado);
                        limit = 0;
                        anterior = auxiliar;
                        listaMarcas = new List<Marca> ();
                        marca = new Marca () {
                            Nome = dRead[1].ToString ().Trim (),
                            QtdVendasMarca = Convert.ToInt16 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    }
                    contador++;

                }
                estado = new ContagemMarca {
                    nome = "SETA." + pais + "." + auxiliar,
                    lstMarca = listaMarcas
                };
                Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                Console.WriteLine ("Nome: " + estado.nome);
                listEstados.Add (estado);

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listEstados;
        }
        public List<ContagemMarca> listMarcasCidades (string pais, string estado, int qtd) { //localhost:5000/api/geoseta/BR
            List<ContagemMarca> listCidades = new List<ContagemMarca> (); //Cria uma lista de Contagem para posteriormente passar como JSON para o FrontEnd
            ContagemMarca cidade = new ContagemMarca ();
            int contador = 0;
            try {
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                // Colocar o Pais
                // Decidir se vai fazer o gasto médio por bairro ou se vai fazer o gasto total, se for o médio é só pegar o total que vem e dividir pela quantidade de pessoas          
                string sql = String.Format ("select cid.descricao, mar.descricao, sum(m.quantidade) from pessoas as p join cepcidades as cid on p.codcidade = cid.codigo join vendas as v on v.cliente = p.codigo join movimento as m on SUBSTR(m.auxiliar, 3, 8)::Char(8) = v.codigo AND m.operacao = 'VE' join produtos as pro on pro.codigo = substr(m.produto, 1, 6)::Char(6) join marcas as mar on mar.codigo = pro.marca where cid.uf = '{0}' group by cid.descricao, mar.descricao order by cid.descricao, sum desc", estado);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                string auxiliar = "";
                string anterior = "";
                List<Marca> listaMarcas = new List<Marca> ();
                Marca marca;
                int limit = 0;
                while (dRead.Read ()) {
                    auxiliar = dRead[0].ToString ().Trim ();
                    if (contador == 0) {
                        anterior = auxiliar;
                        marca = new Marca () {
                            Nome = dRead[1].ToString ().Trim (),
                            QtdVendasMarca = Convert.ToInt16 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    }

                    if (contador != 0 && auxiliar == anterior && limit < qtd) {
                        marca = new Marca {
                        Nome = dRead[1].ToString ().Trim (),
                        QtdVendasMarca = Convert.ToInt16 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    } else if (contador != 0 && auxiliar != anterior) {

                        cidade = new ContagemMarca {
                        nome = "SETA." + pais + "." + estado + "." + anterior,
                        lstMarca = listaMarcas
                        };
                        Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                        Console.WriteLine ("Nome: " + cidade.nome);
                        listCidades.Add (cidade);
                        limit = 0;
                        anterior = auxiliar;
                        listaMarcas = new List<Marca> ();
                        marca = new Marca () {
                            Nome = dRead[1].ToString ().Trim (),
                            QtdVendasMarca = Convert.ToInt16 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    }
                    contador++;

                }
                cidade = new ContagemMarca {
                    nome = "SETA." + pais + "." + estado + "." + auxiliar,
                    lstMarca = listaMarcas
                };
                Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                Console.WriteLine ("Nome: " + cidade.nome);
                listCidades.Add (cidade);

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listCidades;
        }
        public List<ContagemMarca> listMarcasBairros (string pais, string estado, string cidade, int qtd) { //localhost:5000/api/geoseta/BR
            List<ContagemMarca> listBairros = new List<ContagemMarca> (); //Cria uma lista de Contagem para posteriormente passar como JSON para o FrontEnd
            ContagemMarca bairro = new ContagemMarca ();
            int contador = 0;
            try {
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                // Colocar o Pais
                // Decidir se vai fazer o gasto médio por bairro ou se vai fazer o gasto total, se for o médio é só pegar o total que vem e dividir pela quantidade de pessoas          
                string sql = String.Format ("select bai.descricao, mar.descricao, sum(m.quantidade) from pessoas as p "+
                "join cep on p.cep = cep.codigo join cepbairros as bai on cep.bairro = bai.codigo " +
                "join cepcidades as cid on cep.cidade = cid.codigo join vendas as v on v.cliente = p.codigo join movimento as m on SUBSTR(m.auxiliar, 3, 8)::Char(8) = v.codigo and m.operacao = 'VE' join produtos as pro on pro.codigo = substr(m.produto, 1, 6)::Char(6) join marcas as mar on mar.codigo = pro.marca where p.codcidade = cep.cidade and p.cep != '' and cid.descricao = '{0}' and cid.uf = '{1}' group by bai.descricao, mar.descricao order by bai.descricao, sum desc", cidade, estado);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                string auxiliar = "";
                string anterior = "";
                List<Marca> listaMarcas = new List<Marca> ();
                Marca marca;
                int limit = 0;
                while (dRead.Read ()) {
                    auxiliar = dRead[0].ToString ().Trim ();
                    if (contador == 0) {
                        anterior = auxiliar;
                        marca = new Marca () {
                            Nome = dRead[1].ToString ().Trim (),
                            QtdVendasMarca = Convert.ToInt16 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    }

                    if (contador != 0 && auxiliar == anterior && limit < qtd) {
                        marca = new Marca {
                        Nome = dRead[1].ToString ().Trim (),
                        QtdVendasMarca = Convert.ToInt16 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    } else if (contador != 0 && auxiliar != anterior) {

                        bairro = new ContagemMarca {
                        nome = "SETA." + pais + "." + estado + "." + cidade + "." + anterior,
                        lstMarca = listaMarcas
                        };
                        Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                        Console.WriteLine ("Nome: " + bairro.nome);
                        listBairros.Add (bairro);
                        limit = 0;
                        anterior = auxiliar;
                        listaMarcas = new List<Marca> ();
                        marca = new Marca () {
                            Nome = dRead[1].ToString ().Trim (),
                            QtdVendasMarca = Convert.ToInt16 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    }
                    contador++;

                }
                bairro = new ContagemMarca {
                    nome = "SETA." + pais + "." + estado + "." + cidade + "." + auxiliar,
                    lstMarca = listaMarcas
                };
                Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                Console.WriteLine ("Nome: " + bairro.nome);
                listBairros.Add (bairro);

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listBairros;
        }
        public List<ContagemMarca> listMarcasCidadeSomente (string pais, string estado, string cidade, int qtd) { //localhost:5000/api/geoseta/BR
            List<ContagemMarca> listBairros = new List<ContagemMarca> (); //Cria uma lista de Contagem para posteriormente passar como JSON para o FrontEnd
            ContagemMarca bairro = new ContagemMarca ();
            int contador = 0;
            try {
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();


                // Colocar o Pais
                // Decidir se vai fazer o gasto médio por bairro ou se vai fazer o gasto total, se for o médio é só pegar o total que vem e dividir pela quantidade de pessoas          
                string sql = String.Format ("select cid.descricao, mar.descricao, sum(m.quantidade) from pessoas as p join cepcidades as cid on p.codcidade = cid.codigo join vendas as v on v.cliente = p.codigo join movimento as m on SUBSTR(m.auxiliar, 3, 8)::Char(8) = v.codigo AND m.operacao = 'VE' join produtos as pro on pro.codigo = substr(m.produto, 1, 6)::Char(6) join marcas as mar on mar.codigo = pro.marca where cid.uf = 'PR' and cid.descricao = 'LONDRINA' group by cid.descricao, mar.descricao order by sum desc", estado, cidade);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();
                List<Marca> listaMarcas = new List<Marca> ();
                Marca marca;
                int limit = 0;
                while (dRead.Read ()) {
                    if(limit<=qtd){
                        marca = new Marca () {
                            Nome = dRead[1].ToString ().Trim (),
                            QtdVendasMarca = Convert.ToInt32 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    }
                    else break; 
                }
                bairro = new ContagemMarca {
                    nome = "SETA." + pais + "." + estado + "." + cidade,
                    lstMarca = listaMarcas
                };
                Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                Console.WriteLine ("Nome: " + bairro.nome);
                listBairros.Add (bairro);

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listBairros;
        }
        public  void TopMarcas (int qtd) { //localhost:5000/api/geoseta/BR
            this.Marcas = new List<Contagem> (); //Cria uma lista de Contagem para posteriormente passar como JSON para o FrontEnd
            Contagem var = new Contagem ();
            int contador = 0;
            try {
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                // Colocar o Pais
                // Decidir se vai fazer o gasto médio por bairro ou se vai fazer o gasto total, se for o médio é só pegar o total que vem e dividir pela quantidade de pessoas          
                string sql = String.Format ("select  mar.descricao, count(m.quantidade) from pessoas as p " +
                    "join vendas as v on v.cliente = p.codigo join movimento as m on SUBSTR(m.auxiliar, 3, 8)::Char(8) = v.codigo " +
                    "AND m.operacao = 'VE' join produtos as pro on pro.codigo = substr(m.produto, 1, 6)::Char(6) " +
                    "join marcas as mar on mar.codigo = pro.marca where p.UF != '' " +
                    "and p.UF != 'EX' and p.cliente = 't' " +
                    "group by  mar.descricao order by  count desc limit {0}", qtd);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                while (dRead.Read ()) {
                    contador++;
                    var = new Contagem {
                        nome = dRead[0].ToString ().Trim (),
                        valor = Convert.ToInt32 (dRead[1].ToString ().Trim ())
                    };
                    Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                    Console.WriteLine ("Nome: " + var.nome + "\nContagem" + var.valor);
                    this.Marcas.Add (var);

                }
            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
        }
        public List<ContagemMarca> listTopMarcasUFs (string pais, int qtd) { //localhost:5000/api/geoseta/BR
            List<ContagemMarca> listEstados = new List<ContagemMarca> (); //Cria uma lista de Contagem para posteriormente passar como JSON para o FrontEnd
            ContagemMarca estado = new ContagemMarca ();
            int contador = 0;
            try {
                
                this.TopMarcas(7);
                Console.WriteLine(this.Marcas[0].nome+" outra "+this.Marcas[1].nome+" outra "+this.Marcas[2].nome+" outra "+this.Marcas[3].nome+" outra "+this.Marcas[4].nome+" outra "+this.Marcas[5].nome+" outra "+this.Marcas[6].nome);
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                // Colocar o Pais
                // Decidir se vai fazer o gasto médio por bairro ou se vai fazer o gasto total, se for o médio é só pegar o total que vem e dividir pela quantidade de pessoas          
                string sql = String.Format ("select p.UF, mar.descricao, count(m.quantidade) from pessoas as p " +
                    "join vendas as v on v.cliente = p.codigo join movimento as m on SUBSTR(m.auxiliar, 3, 8)::Char(8) = v.codigo " +
                    "AND m.operacao = 'VE' join produtos as pro on pro.codigo = substr(m.produto, 1, 6)::Char(6) " +
                    "join marcas as mar on mar.codigo = pro.marca where p.UF != ''  " +
                    "and p.UF != 'EX' and p.cliente = 't' AND (mar.descricao = '{0}' or mar.descricao = '{1}' OR mar.descricao = '{2}' OR mar.descricao = '{3}' OR mar.descricao = '{4}' OR mar.descricao = '{5}' OR mar.descricao = '{6}')" +
                    "group by  p.uf, mar.descricao order by  count desc",this.Marcas[0].nome,this.Marcas[1].nome,this.Marcas[2].nome,this.Marcas[3].nome,this.Marcas[4].nome,this.Marcas[5].nome,this.Marcas[6].nome);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                string auxiliar = "";
                string anterior = "";
                List<Marca> listaMarcas = new List<Marca> ();
                Marca marca;
                int limit = 0;
                while (dRead.Read ()) {
                    auxiliar = dRead[0].ToString ().Trim ();
                    if (contador == 0) {
                        anterior = auxiliar;
                        marca = new Marca () {
                            Nome = dRead[1].ToString ().Trim (),
                            QtdVendasMarca = Convert.ToInt16 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    }

                    if (contador != 0 && auxiliar == anterior && limit < qtd) {
                        marca = new Marca {
                            Nome = dRead[1].ToString ().Trim (),
                            QtdVendasMarca = Convert.ToInt16 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    } else if (contador != 0 && auxiliar != anterior) {

                        estado = new ContagemMarca {
                            nome = "SETA." + pais + "." + anterior,
                            lstMarca = listaMarcas
                        };
                        Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                        Console.WriteLine ("Nome: " + estado.nome);
                        listEstados.Add (estado);
                        limit = 0;
                        anterior = auxiliar;
                        listaMarcas = new List<Marca> ();
                        marca = new Marca () {
                            Nome = dRead[1].ToString ().Trim (),
                            QtdVendasMarca = Convert.ToInt16 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    }
                    contador++;

                }
                estado = new ContagemMarca {
                    nome = "SETA." + pais + "." + auxiliar,
                    lstMarca = listaMarcas
                };
                Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                Console.WriteLine ("Nome: " + estado.nome);
                listEstados.Add (estado);

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listEstados;
        }
        public List<ContagemMarca> listTopMarcasCidades (string pais, string estado, int qtd) { //localhost:5000/api/geoseta/BR
            List<ContagemMarca> listCidades = new List<ContagemMarca> (); //Cria uma lista de Contagem para posteriormente passar como JSON para o FrontEnd
            ContagemMarca cidade = new ContagemMarca ();
            int contador = 0;
            try {
                
                //this.TopMarcas(7);
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                // Colocar o Pais
                // Decidir se vai fazer o gasto médio por bairro ou se vai fazer o gasto total, se for o médio é só pegar o total que vem e dividir pela quantidade de pessoas          
                string sql = String.Format ("select cid.descricao, mar.descricao, sum(m.quantidade) from pessoas as p " +
                    "join cepcidades as cid on p.codcidade = cid.codigo " +
                    "join vendas as v on v.cliente = p.codigo join movimento as m on SUBSTR(m.auxiliar, 3, 8)::Char(8) = v.codigo AND m.operacao = 'VE' " +
                    "join produtos as pro on pro.codigo = substr(m.produto, 1, 6)::Char(6) " +
                    "join marcas as mar on mar.codigo = pro.marca " +
                    "where cid.uf = '{0}' and (mar.descricao = '{1}' or mar.descricao = '{2}' OR mar.descricao = '{3}' OR mar.descricao = '{4}' OR mar.descricao = '{5}' OR mar.descricao = '{6}' OR mar.descricao = '{7}') group by cid.descricao, mar.descricao " +
                    "order by cid.descricao, sum desc",estado,this.Marcas[0].nome,this.Marcas[1].nome,this.Marcas[2].nome,this.Marcas[3].nome,this.Marcas[4].nome,this.Marcas[5].nome,this.Marcas[6].nome);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                string auxiliar = "";
                string anterior = "";
                List<Marca> listaMarcas = new List<Marca> ();
                Marca marca;
                int limit = 0;
                while (dRead.Read ()) {
                    auxiliar = dRead[0].ToString ().Trim ();
                    if (contador == 0) {
                        anterior = auxiliar;
                        marca = new Marca () {
                            Nome = dRead[1].ToString ().Trim (),
                            QtdVendasMarca = Convert.ToInt16 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    }

                    if (contador != 0 && auxiliar == anterior && limit < qtd) {
                        marca = new Marca {
                        Nome = dRead[1].ToString ().Trim (),
                        QtdVendasMarca = Convert.ToInt16 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    } else if (contador != 0 && auxiliar != anterior) {

                        cidade = new ContagemMarca {
                        nome = "SETA." + pais + "." + estado + "." + anterior,
                        lstMarca = listaMarcas
                        };
                        Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                        Console.WriteLine ("Nome: " + cidade.nome);
                        listCidades.Add (cidade);
                        limit = 0;
                        anterior = auxiliar;
                        listaMarcas = new List<Marca> ();
                        marca = new Marca () {
                            Nome = dRead[1].ToString ().Trim (),
                            QtdVendasMarca = Convert.ToInt16 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    }
                    contador++;

                }
                cidade = new ContagemMarca {
                    nome = "SETA." + pais + "." + estado + "." + auxiliar,
                    lstMarca = listaMarcas
                };
                Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                Console.WriteLine ("Nome: " + cidade.nome);
                listCidades.Add (cidade);

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listCidades;
        }
        public List<ContagemMarca> listTopMarcasBairros (string pais, string estado, string cidade, int qtd) { //localhost:5000/api/geoseta/BR
            List<ContagemMarca> listBairros = new List<ContagemMarca> (); //Cria uma lista de Contagem para posteriormente passar como JSON para o FrontEnd
            ContagemMarca bairro = new ContagemMarca ();
            int contador = 0;
            try {
                
                //this.TopMarcas(7);
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                // Colocar o Pais
                // Decidir se vai fazer o gasto médio por bairro ou se vai fazer o gasto total, se for o médio é só pegar o total que vem e dividir pela quantidade de pessoas          
                string sql = String.Format ("select bai.descricao, mar.descricao, sum(m.quantidade) from pessoas as p " +
                "join cep on p.cep = cep.codigo join cepbairros as bai on cep.bairro = bai.codigo " +
                "join cepcidades as cid on cep.cidade = cid.codigo join vendas as v on v.cliente = p.codigo " +
                "join movimento as m on SUBSTR(m.auxiliar, 3, 8)::Char(8) = v.codigo and m.operacao = 'VE' " +
                "join produtos as pro on pro.codigo = substr(m.produto, 1, 6)::Char(6) " +
                "join marcas as mar on mar.codigo = pro.marca where p.codcidade = cep.cidade and p.cep != '' and cid.descricao = '{0}' and cid.uf = '{1}' " +
                "and (mar.descricao = '{2}' or mar.descricao = '{3}' OR mar.descricao = '{4}' OR mar.descricao = '{5}' OR mar.descricao = '{6}' OR mar.descricao = '{7}' OR mar.descricao = '{8}') " +
                "group by bai.descricao, mar.descricao order by bai.descricao, sum desc",cidade,estado,this.Marcas[0].nome,this.Marcas[1].nome,this.Marcas[2].nome,this.Marcas[3].nome,this.Marcas[4].nome,this.Marcas[5].nome,this.Marcas[6].nome);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                string auxiliar = "";
                string anterior = "";
                List<Marca> listaMarcas = new List<Marca> ();
                Marca marca;
                int limit = 0;
                while (dRead.Read ()) {
                    auxiliar = dRead[0].ToString ().Trim ();
                    if (contador == 0) {
                        anterior = auxiliar;
                        marca = new Marca () {
                            Nome = dRead[1].ToString ().Trim (),
                            QtdVendasMarca = Convert.ToInt16 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    }

                    if (contador != 0 && auxiliar == anterior && limit < qtd) {
                        marca = new Marca {
                        Nome = dRead[1].ToString ().Trim (),
                        QtdVendasMarca = Convert.ToInt16 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    } else if (contador != 0 && auxiliar != anterior) {

                        bairro = new ContagemMarca {
                        nome = "SETA." + pais + "." + estado + "." + cidade + "." + anterior,
                        lstMarca = listaMarcas
                        };
                        Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                        Console.WriteLine ("Nome: " + bairro.nome);
                        listBairros.Add (bairro);
                        limit = 0;
                        anterior = auxiliar;
                        listaMarcas = new List<Marca> ();
                        marca = new Marca () {
                            Nome = dRead[1].ToString ().Trim (),
                            QtdVendasMarca = Convert.ToInt16 (dRead[2].ToString ().Trim ())
                        };
                        listaMarcas.Add (marca);
                        limit++;
                    }
                    contador++;

                }
                bairro = new ContagemMarca {
                    nome = "SETA." + pais + "." + estado + "." + cidade + "." + anterior,
                    lstMarca = listaMarcas
                };
                Console.WriteLine ("Adicionando Pessoa Nº: " + contador);
                Console.WriteLine ("Nome: " + bairro.nome);
                listBairros.Add (bairro);

            } catch (Exception msg) {
                Console.WriteLine (" Erro em Listar Todos" + msg.ToString ());
            }
            return listBairros;
        }
        //Fim dos novos métodos/////////////////////////////////////////////////////////

        public Pessoa BuscarPessoa (string codigo) {
            Pessoa pessoa = new Pessoa (); //inicialização de Objeto normal (estilo Java)
            try {
                //Abro conexão com o banco!
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                //Aqui mostro como passar parametro :D
                string sql = String.Format ("SELECT codigo, nome, cpfcnpj, status FROM pessoas WHERE codigo = '{0}'", codigo);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                if (dRead.Read ()) {
                    //inicialização de Objeto que deve ser usada (Padrão C#)
                    pessoa = new Pessoa {
                        Codigo = dRead[0].ToString ().Trim (),
                        Nome = dRead[1].ToString ().Trim (),
                        Cpf = dRead[2].ToString ().Trim (),
                        Status = dRead[3].ToString ().Trim ()
                    };
                }

                //Fecho Conexão com o banco!
                conn.Close ();
            } catch (Exception msg) {
                //Deu pau!!! O que pode ter acontecido?!?!
                Console.WriteLine (msg.ToString ());
            }
            return pessoa;
        }

        public string CadastrarPessoa (Pessoa pessoa) {
            try {
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                //Aqui mostro como passar parametro :D
                string sql = String.Format ("INSERT INTO pessoas (nome, pessoa, cep, endereco, bairro, cidade, uf, cpfcnpj, estadocivil, status, sexo, cadastro, cliente) VALUES ('{0}',	'{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}','{10}', '{11}', '{12}') RETURNING codigo", pessoa.Nome, pessoa.Tipo, pessoa.Cep, pessoa.Endereco, pessoa.Bairro, pessoa.Cidade, pessoa.Uf, pessoa.Cpf, pessoa.Estadocivil, pessoa.Status, pessoa.Sexo, pessoa.Cadastro, pessoa.Cliente);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                if (dRead.Read ()) {
                    //inicialização de Objeto que deve ser usada (Padrão C#)
                    pessoa.Codigo = dRead[0].ToString ().Trim ();
                }

                //Fecho Conexão com o banco!
                conn.Close ();
            } catch (Exception msg) {
                //Deu pau!!! O que pode ter acontecido?!?!
                Console.WriteLine (msg.ToString ());
            }
            return pessoa.Codigo;
        }

        public string CadastrarPreVenda (Venda prevenda) {
            try {
                //Abro conexão com o banco!
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                //Aqui mostro como passar parametro :D
                string sql = String.Format ("INSERT INTO vendas (empresa, tipo, cliente, vendedor, condicoes, status, data, hora, itens, subtotal, total, pecas, clientestatus, prevenda, cpfcnpj) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}','{5}', '{6}', '{7}', {8}, {9}, {10}, {11}, '{12}', '{13}', '{14}') RETURNING codigo",
                    prevenda.Empresa, prevenda.Tipo, prevenda.Cliente, prevenda.Vendedor, prevenda.Condicoes, prevenda.Status, prevenda.Data, prevenda.Hora, prevenda.Itens, prevenda.SubTotal.ToString ().Replace (",", "."), prevenda.Total.ToString ().Replace (",", "."), prevenda.Itens, prevenda.ClienteStatus, prevenda.PreVenda, prevenda.Cpf);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                if (dRead.Read ()) {
                    //inicialização de Objeto que deve ser usada (Padrão C#)
                    prevenda.Codigo = dRead[0].ToString ().Trim ();
                }

                //Fecho Conexão com o banco!
                conn.Close ();
            } catch (Exception msg) {
                //Deu pau!!! O que pode ter acontecido?!?!
                Console.WriteLine (msg.ToString ());
            }
            return prevenda.Codigo;
        }

        public string CadastrarMovimentoPreVenda (Movimento movimento) {
            try {
                //Abro conexão com o banco!
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                //Aqui mostro como passar parametro :D
                string sql = String.Format ("INSERT INTO movimento (auxiliar, operacao, movimento, data, empresa, produto, quantidade, unitario, total, vendedorm, custo) VALUES ('{0}','{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', {10}) RETURNING codigo", movimento.Auxiliar, movimento.Op, movimento.Mov, movimento.Data, movimento.Empresa, movimento.CodProduto, movimento.Qtd, movimento.Unitario.ToString ().Replace (",", "."), movimento.Total.ToString ().Replace (",", "."), movimento.Vendedor, movimento.Custo.ToString ().Replace (",", "."));
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                if (dRead.Read ()) {
                    //inicialização de Objeto que deve ser usada (Padrão C#)
                    movimento.Codigo = dRead[0].ToString ().Trim ();
                }

                //Fecho Conexão com o banco!
                conn.Close ();
            } catch (Exception msg) {
                //Deu pau!!! O que pode ter acontecido?!?!
                Console.WriteLine (msg.ToString ());
            }
            return movimento.Codigo;
        }
        public decimal BuscarInfoProduto (string codigo, string opcao) {
            Pessoa pessoa = new Pessoa (); //inicialização de Objeto normal (estilo Java)
            decimal preco = 0;
            try {
                //Abro conexão com o banco!
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                //Aqui mostro como passar parametro :D
                string sql = String.Format ("SELECT {0} FROM produtos WHERE codigo = '{1}'", opcao, codigo);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                if (dRead.Read ()) {
                    //inicialização de Objeto que deve ser usada (Padrão C#)
                    preco = dRead.GetDecimal (0);
                }

                //Fecho Conexão com o banco!
                conn.Close ();
            } catch (Exception msg) {
                //Deu pau!!! O que pode ter acontecido?!?!
                Console.WriteLine (msg.ToString ());
            }
            return preco;
        }
        public void AlterarPessoa (Pessoa pessoa) {

            try {
                //Abro conexão com o banco!
                NpgsqlConnection conn = new NpgsqlConnection (connstring);
                conn.Open ();

                //Aqui mostro como passar parametro :D
                string sql = String.Format ("UPDATE pessoas SET status = '{0}' WHERE codigo = '{1}'", pessoa.Status, pessoa.Codigo);
                NpgsqlCommand cmd = new NpgsqlCommand (sql, conn);
                NpgsqlDataReader dRead = cmd.ExecuteReader ();

                //Fecho Conexão com o banco!
                conn.Close ();
            } catch (Exception msg) {
                //Deu pau!!! O que pode ter acontecido?!?!
                Console.WriteLine (msg.ToString ());
            }
        }

    }
}