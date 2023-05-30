<?PHP

// OSHMI/Open Substation HMI - Copyright 2008-2018 - Ricardo L. Olsen

// Read and write annotations to a database (notes.sl3)

// Parameters:
//
// &N=POINTNUM &W=WRITE &FILTAG &FILCONTENT
// POINTNUM : point number
// WRITE : 1 = write 
//
// OR
// FILTAG : tag filter
// FILCONTENT : content filter

// insert into notes (ERASED, OPENED, POINTNUM, CONTENT, TSCREATE, TSERASE) values (0,0,11388,"teste nota",0,0);

header("Content-type: application/json");
header("Content-Disposition: inline; filename=\"annotation.json\"");
header("Cache-Control: no-store, must-revalidate");
header('Access-Control-Allow-Origin: *');

// Show no errors
error_reporting(0);
    
require_once 'timezone.php';

extract($_REQUEST, EXTR_PREFIX_ALL|EXTR_SKIP, 'p');

if ( ! ( isset( $p_N ) || isset( $p_FILTAG ) || isset( $p_FILCONTENT ) ) )
  return;

$pointnum = intval($p_N);

$write = 0;
if ( isset( $p_W ) )
  $write = $p_W ? 1 : 0;

$filtag = "";
if ( isset( $p_FILTAG ) )
  $filtag = $p_FILTAG;

$filcontent = "";
if ( isset( $p_FILCONTENT ) )
  $filcontent = $p_FILCONTENT;

if ( $write && isset( $p_CONTENT ) )
  {
  $p_CONTENT = trim($p_CONTENT);

  // Forward request to redundant HMI if exists
  $otherhmiip="";
  $port=51909;
  // Parse with sections
  $ini_array = parse_ini_file("../conf/hmi.ini", true);
  if ( $ini_array["REDUNDANCY"]["OTHER_HMI_IP"] != "" )
    $otherhmiip = $ini_array["REDUNDANCY"]["OTHER_HMI_IP"];
  if ( $ini_array["REDUNDANCY"]["HTTP_PORT"] != "" )
    $port = $ini_array["REDUNDANCY"]["HTTP_PORT"];
  if ( $otherhmiip != "" )
  if ( $_SERVER['REMOTE_ADDR'] != $otherhmiip )
    {
    $ch = curl_init();
    curl_setopt($ch, CURLOPT_URL,"http://$otherhmiip:$port/htdocs/annotation.php?N=$p_N&W=$p_W");
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, false);
    curl_setopt($ch, CURLOPT_POST, 1);
    curl_setopt($ch, CURLOPT_POSTFIELDS, http_build_query(array('CONTENT' => $p_CONTENT)));
    curl_setopt($ch, CURLOPT_NOSIGNAL, 1);
    curl_setopt($ch, CURLOPT_TIMEOUT_MS, 1000);
    curl_setopt($ch, CURLOPT_CONNECTTIMEOUT, 500);
    curl_exec($ch);
    curl_close($ch);      
    }
  }

try {
    if ( $write ) 
      {
      $pdo = new PDO( 'sqlite:../db/notes.sl3','','', [
        PDO::ATTR_TIMEOUT => 5,
        PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION
        ] );
      $pdo->exec ( "PRAGMA synchronous=NORMAL" );
      $pdo->exec ( "PRAGMA journal_mode=WAL" );
      $pdo->exec ( "PRAGMA locking_mode=NORMAL" );
      $pdo->exec ( "PRAGMA cache_size=5000" );
      $pdo->exec ( "PRAGMA temp_store=MEMORY" );
      }
    else
      {
      $pdo = new PDO( 'sqlite:../db/notes.sl3','','', [
        PDO::ATTR_TIMEOUT => ($filtag!="" || $filcontent!="")?0:5,
        // PDO::SQLITE_ATTR_OPEN_FLAGS => PDO::SQLITE_OPEN_READONLY, // THIS FLAG CAUSES ERRORS
        PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION
        ] );
      $pdo->exec ( "PRAGMA query_only=TRUE" );
      $pdo->exec ( "PRAGMA synchronous=NORMAL" );
      $pdo->exec ( "PRAGMA journal_mode=WAL" );
      $pdo->exec ( "PRAGMA locking_mode=NORMAL" );
      $pdo->exec ( "PRAGMA cache_size=10000" );
      $pdo->exec ( "PRAGMA temp_store=MEMORY" );
      if ($filtag!="" || $filcontent!="")
        $pdo->exec ( "ATTACH DATABASE '../db/dumpdb.sl3' as DBPONTOS" );
      }  

    if ( $write )
      {
      $pdo->query("BEGIN TRANSACTION");
      $now_tm = time();
      $qry = "update notes set ERASED = 1, TSERASE = :now_tm where POINTNUM = :pointnum and ERASED = 0";
      $stmt = $pdo->prepare( $qry );
      $stmt->execute(array( 'pointnum' => $pointnum,
		                        'now_tm' => $now_tm
                          ));
      if ( $p_CONTENT != "" )
        {
        $rmt_ip =  $_SERVER['REMOTE_ADDR'];  
        $qry = "insert into notes (ERASED, OPENED, POINTNUM, CONTENT, TSCREATE, TSERASE, USER) values (0, 0, :pointnum, :content, :now_tm, 0, :rmt_ip)";
		    $stmt = $pdo->prepare( $qry );
        $stmt->execute(array( 'pointnum' => $pointnum,
		                      'content' => $p_CONTENT,
							            'now_tm' => $now_tm,
						           	  'rmt_ip' => $rmt_ip
                          ));
        }
      $pdo->query("COMMIT");
      }
    else
      {  
 
      if ($filtag!="" or $filcontent!="")
		  {
		  $stmt = $pdo->prepare(
			"SELECT 
			   n.ID as REGNUM, 
			   n.POINTNUM as POINTNUM, 
			   n.CONTENT as CONTENT, 
			   n.TSCREATE as TSCREATE
			 FROM notes n
			 JOIN dumpdb d on n.POINTNUM = d.NPONTO and n.ERASED = 0
			 WHERE 
			   d.id like :filtag and n.CONTENT like :filcontent 
			 ORDER BY n.ID DESC  
			");
		  $stmt->setFetchMode(PDO::FETCH_ASSOC);
		  $stmt->execute( array('filtag' => $filtag."%",
				                'filcontent' => "%".$filcontent."%" 
						 ) );
		  }
	  else
		  {
		  $stmt = $pdo->prepare(
			"SELECT 
			   n.ID as REGNUM, 
			   n.POINTNUM as POINTNUM, 
			   n.CONTENT as CONTENT, 
			   n.TSCREATE as TSCREATE
			 FROM notes n
			 WHERE 
			   n.POINTNUM = :pointnum and 
			   n.ERASED = 0
			 ORDER BY n.ID DESC  
			 LIMIT 1          
			");
		  $stmt->setFetchMode(PDO::FETCH_ASSOC);
		  $stmt->execute(array('pointnum' => $pointnum ));
		  }
      
      $arr = array(); 
      foreach ($stmt as $row)  
        { 
        $arr[]=$row;
        }

      $res = json_encode ($arr, JSON_NUMERIC_CHECK | JSON_UNESCAPED_UNICODE);  
      if ($res===false)
        {
          switch (json_last_error()) {
              case JSON_ERROR_NONE:
                  echo ' - No errors';
              break;
              case JSON_ERROR_DEPTH:
                  echo ' - Maximum stack depth exceeded';
              break;
              case JSON_ERROR_STATE_MISMATCH:
                  echo ' - Underflow or the modes mismatch';
              break;
              case JSON_ERROR_CTRL_CHAR:
                  echo ' - Unexpected control character found';
              break;
              case JSON_ERROR_SYNTAX:
                  echo ' - Syntax error, malformed JSON';
              break;
              case JSON_ERROR_UTF8:
                  echo ' - Malformed UTF-8 characters, possibly incorrectly encoded';
              break;
              default:
                  echo ' - Unknown error';
              break;
          }  
        }
      else
        echo $res;
      }
    }
catch( PDOException $e )
    {
    echo "{\"error\":\"Error reading database!\"}";
    // echo "Erro: ".$e->getMessage();
    // print_r($dbsde->errorInfo());
    }

?>
