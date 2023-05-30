// Internationalization Messages
// OSHMI/Open Substation HMI - Copyright (c) 2008-2023 - Ricardo L. Olsen

var Msg =
{
NomeProduto: 'OSHMI',
VersaoProduto: '6.29',

NomeVisorTelas:       'Visor de Telas',
NomeVisorEventos:     'Visor de Eventos',
NomeVisorHistorico:   'Visor de Eventos - Histórico',
NomeVisorTabular:     'Visor Tabular', 
NomeVisorAnormais:    'Visor de Alarmes',
NomeVisorTendencias:  'Visor de Tendências',  
NomeVisorCurvas:      'Visor de Curvas',  
NomeDialogoInfo:      'Acesso ao Ponto',  
NomeDialogoCmd:       'Comando',  

FalhaWebserver:  'Falha no servidor Web!',

// websage.js
BlqAnot:     'Acesso ao comando bloqueado automaticamente por presença de anotação.',
AcessCmd:    'Pressione para ter acesso ao comando.',
EstadoAtual: 'estado atual',
ConfNSuport: 'Configuração não suportada!',
BrowserNSup: 'Browser não suportado!',
Qualific:    'Qualific',
QFalhado:    'FALHADO',
QSubst:      'SUBSTITUIDO',
QCalculado:  'CALCULADO',
QManual:     'MANUAL',
QNuncaAtu:   'NUNCA_ATUALIZADO',
QAlarmado:   'ALARMADO',
QAnotacao:   'ANOTACAO',
QAlmInib:    'ALM.INIBIDO',
QNaoNormal:  'ALM.PERSISTENTE',
QLimiteViol: 'LIMITE_VIOLADO',
QCongelado:  'CONGELADO',
QNormal:     'NORMAL',
QDPIntermed: 'INTERMEDIARIO',
QDPInvalido: 'INVALIDO',
QValor:      'valor',
SELTELA_OPC1:'Escolha a tela ...',

// Events.html
Eventos:       'Eventos',
ModoNormal:    'Modo Normal',
ModoAgregado:  'Modo Agregado',
ModoPanico:    'Modo Pânico',
ModoCongelado: 'CONGELADO!',
ModoHistorico: 'Histórico',
HIST:          '>Histórico de Eventos',
SPDATAINI:     'Data: ',
SPHORAINI:     'Hora Inicial: ',
SPFILTRO:      'Filtro: ',
btBuscaHist:   'Buscar',
EveNomesColunas: 'Data,Hora,ms,nponto,ID,Subest.,Descrição,Evento,Flags,Qualif.',
ConfirmaSaida: 'Realmente deseja fechar o visor de eventos?',
EveFiltradosSE:'FILTRADOS',

// tabular.html
SPCOMANDAVEIS: 'Comandáveis',
SPANORMAIS:    'Alarmes',
SPSUBEST:      'Subest',
SPMODULO:      'Módulo',
SPFILTROID:    'Filtro (ID)',
SELSE:         'Escolha a subestação',
SELMOD:        'Escolha o módulo para filtrar',
Filtro:        'Filtra pelo ID', 
TabNomesColunas: 'NPonto,ID,Subest.,Descr.,Est/Val Atual,Flg,Cmd,Qualif.,Nor,Hora Alarme',
SelectAll:     'Marcar Todos',
UnselectAll:   'Desmarcar Todos',

// dlginfo.html
TENDTXT:     '>Acompanhar medida',
TABULARTXT:  '>Tela tabular do módulo',
CURVTXT:     '>Abrir no visor de curvas',
ANOTACAOTXT: 'Anotação:',
DESBLOQTXT:  'Desbloqueia Comando',
COMANDAR:    'COMANDAR',
CBMOREINFO:  '+ Outras Opções',
SPPONTOSUP:  'Ponto Supervisionado:',
ALRINTXT:    'Alarme Inibido',
SPLIMSUP:    'Limite Superior',
SPLIMINF:    'Limite Inferior',
SPLIMHIS:    'Histerese',
SPALTVALOR:  'Altera Valor',
SAIR:        'SAIR',
CANCELAR:    'CANCELAR',
FSINFO:      'Informação',
FSBLKANNOT:  'Anotação de Bloqueio',
FSANNOT:     'Anotação Documental',
FSOPTION:    'Opções',


// dlgcomando.html
SPPASSO2:    'Escolha a função e clique o botão para comandar.',
CMDMOREINFO: '+ Outras informações',
SPPONTOCMD:  'Ponto de Comando:',
FSCMDINFO:   'Objeto do Comando',
FSCMDACTION: 'Ação de Comando',
FSCMDOPTION: 'Opções',

// trend.html
yaxisleft:    'Mostra valores menores [seta para cima]',
yaxisright:   'Mostra valores maiores [seta para baixo]',
yaxiszoomout: 'Menos zoom [-]',
yaxiszoomin:  'Mais zoom [+]',
yaxisminus:   'Reduz altura do gráfico [1]',
yaxisplus:    'Aumenta altura [2]',
yaxiscolor:   'Muda a cor do gráfico [3]',
xaxisleft:    'Retorna no tempo [seta para esquerda]',
xaxisright:   'Avança no tempo [seta para direita]',
xaxiszoomout: 'Menos zoom [/]',
xaxiszoomin:  'Mais zoom [*]',
xaxisminus:   'Reduz largura do gráfico [<]',
xaxisplus:    'Aumenta largura do gráfico [>]',
plotreset:    'Retorna ao zoom normal [0] [Num 5]',
ValorAtual:   'Valor Atual',

Fim: ""
};

var Titles =
{
// screen.html
ZOOMIN_ID :  'Aumenta [Num +]',
ZOOMOUT_ID:  'Reduz [Num -]',
ZPSobe:      'Move para cima [Num 8]',
ZPDesce:     'Move para baixo [Num 2]',
ZPEsq:       'Move para esquerda [Num 4]',
ZPCentro:    'Centraliza [Num 5]',
ZPDir:       'Move para direita [Num 6]',
PRODUTO_ID:  ( Msg.NomeVisorTelas + ' - ' + Msg.NomeProduto ),
TELAS_ID:    'Telas -->',
SELTELA:     'Escolha a tela: [<]=tela anterior, [>]=tela posterior, [1]=primeira tela, [2]=segunda tela, ..., [0]=décima tela ',
CORFUNDO_ID: 'Clique para alterar a cor de fundo.',
AJUDA_ID:    'Ajuda',
HORA_ATU:    'Hora da última atualização dos dados de supervisão. Se ficar antigo tecle [F5]!',
ANORM_ID:    'Mostra Visor de Alarmes (alarmes não reconhecidos mais os persistentes)',
SILENCIA_ID: 'Silencia Bipe! [F9]',
SP_ALARMES:  'Há eventos não reconhecidos! Clique para abrir o Visor de Eventos.',
VENTOINHA:   'Se esta ventoinha parar, tecle [F5]!',
PROXTELAID:  'Próxima tela',
ANTETELAID:  'Tela anterior',
PLAY_ID:     'Inicia Slideshow',
PAUSE_ID:    'Pausa Slideshow',
TIMEMACHINE_ID: 'Máquina do Tempo',
TIMEMACHINECLOSE_ID: 'Fechar Máquina do Tempo',

// dlginfo.html
VALOR_HID:   'valor atual',
ESTADO_HID:  'estado atual',
SPQUALIF:    'Qualificadores do ponto',
SPDESCR_SUP: 'Descritivo do ponto',
SPCMDINTERTRAV: 'COMANDO INTERTRAVADO!', 
IMGANOT:     'Anotação',
ANOTACAO:    'Texto da Anotação',
TABULAR:     'Abre tela tabular do módulo.',
TENDENCIAS:  'Abre visor de tendências.',
DIVBLKCMD:   'Marque para desbloquear intencionalmente comando (bloqueado pela presença de anotação).',
COMANDAR:    'Pressione para ter acesso ao comando.',
CBMOREINFO:  'Clique para mostrar mais informações/opções.',
NPONTO_SUP:  'Número e identificador do ponto.',
VLRNORCTRLS: 'Permite alterar o estado considerado normal.',
DIVINIB:     'Quando marcado, não registra alarme para o ponto.',
LIMCTRLS:    'Configuração de limites para geração alarme de medida fora de faixa.',
LIMSUP:      'Define o valor acima do qual será gerado alarme.',
LIMINF:      'Define o valor abaixo do qual será gerado alarme.',
HISTER:      'Variação mínima do valor para voltar a gerar alarme, após excedido um limite.',
DIVALTVALOR: 'Permite alterar o valor/estado do ponto.',

// dlgcomando.html
COMANDO:     'Selecione o comando desejado.',
EXECUTAR:    'Executa o comando!',
CANCELAR:    'Cancela a execução do comando.',

// events.html
IMGEVENTOS:       ( Msg.NomeVisorEventos + ' - ' + Msg.NomeProduto +  ' - ' + '\nQualificadores: 0-9=Prioridade F=Falhado X=Nunca Atualizado K=Possui Comando A=Anotação L=Alarmado I=Alm.Inibido C=Calculado M=Manual S=Subtituído U=Congelado N=Não Normal Y=Comando Intertravado \n+número=Número de eventos adicionais escondidos quando agregados. \nClique para mostrar NPonto e ID.' ),
imgNormal:        'Modo Normal: mostra todos os eventos que chegam. [1]',
imgAgregar:       'Modo Agregar: agrega os eventos de um mesmo ponto, mostrando somente o último. [2]',
imgPanico:        'Modo Pânico: agrega e mostra somente os eventos mais importantes. [3]',
imgCongelar:      'Modo Congelar: congela a tela (não atualiza) para facilitar a consulta. [4]',
imgHistorico:     'Modo Histórico: consulta de histórico de eventos. [5]',
imgFontSizeUp:    'Aumenta Tamanho da Fonte. [+]',
imgFontSizeDown:  'Reduz Tamanho da Fonte. [-]',
imgReconheceTudo: 'Reconhece Tudo! [F8]. Use [CTRL] + Click do mouse para reconhecer um único alarme.',
imgEliminaTudo:   'Elimina Tudo! [F2]',
imgAlarmes:       'Há eventos não reconhecidos!',
HDATAINI:         'Data para a pesquisa de eventos. [6]=Data Atual',
HHORAINI:         'Hora a partir da qual serão pesquisados os eventos para a data estabelecida. [7]=Zerar',
HFILTRO:          'Filtro para busca dos eventos. Exemplo: CIN = eventos da SE CIN. [8]=Apaga',
imgGpsOnEsc:      'Eventos com hora do GPS. Clique para mostrar a hora local.',
imgGpsOffEsc:     'Eventos com hora local. Clique para mostrar a hora do GPS.',
imgGpsOnFx:       'Eventos com hora do GPS.',
imgGpsOffFx:      'Eventos com hora local.',
imgFilter:        'Filtro de eventos por subestação. Marcar as que se deseja observar.',
imgClipboard:     'Copia para Área de Transferência.',

// tabular.html
LEGQUAL_ID:       '\nQualificadores: 0-9=Prioridade F=Falhado X=Nunca Atualizado K=Possui Comando A=Anotação L=Alarmado I=Alm.Inibido P=Alarme Persistente C=Calculado M=Manual S=Subtituído U=Congelado N=Não Normal Y=Comando Intertravado \nClique para mostrar NPonto e ID.',
IMGTABULAR:       ( Msg.NomeVisorTabular + ' - ' + Msg.NomeProduto +  ' - ' + '\nQualificadores: 0-9=Prioridade F=Falhado X=Nunca Atualizado K=Possui Comando A=Anotação L=Alarmado I=Alm.Inibido P=Alarme Persistente C=Calculado M=Manual S=Subtituído U=Congelado N=Não Normal Y=Comando Intertravado \nClique para mostrar NPonto e ID.' ),
cbMostraCmd:      'Mostra somente os pontos que possuem comando associado. [1]',
cbForaNormal:     'Mostra somente os pontos com estado alarmados. [2]',

Fim: ""
};