<?php
    include_once("./data/storage.php");
    session_start();

    $storage = new Storage(new JsonIO("./data/cards.json"), true);
    $current = null;

    if(isset($_GET["id"])){
        $current = $storage->findById($_GET["id"]);
    }

    if($current == null){
        die("A megadott IKémon azonosító nem létezik!");
    }
?>

<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title><?php echo "IKémon | " . $current["name"] ?></title>
    <link rel="stylesheet" href="./data/styles/main.css">
    <link rel="stylesheet" href="./data/styles/cards.css">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-T3c6CoIi6uLrA9TneNEoa7RxnatzjcDSCmG1MXxSR1GAsXEV/Dwwykc2MPK8M2HN" crossorigin="anonymous">
</head>
<body>
    <header>
        <h1><a href="index.php">IKémon</a> > <?php echo $current["name"] ?></h1>
        <?php
            if(isset($_SESSION["user"])){
                echo '<h2 style="color: white;"><a href="user-details.php">' . $_SESSION["user"] . '</a></h2>';
            } 
            else{
                echo '<h2><a href="login.php">Bejelentkezés</a> | <a href="registration.php">Regisztráció</a></h2>';
            }
        ?>
    </header>
    <div id="content">
        <div id="details">  
            <div class=<?php echo "clr-" . $current["type"] . " image"?>> 
                <img src=<?php echo $current["image"] ?>>
            </div>
            <div class="info">
                <div class="description">
                    <?php echo $current["description"] ?>
                </div>
                <span class="card-type"><span class="icon">🏷</span> Type: <?php echo $current["type"] ?></span>
                <span class="attributes">  
                    <span class="card-hp"><span class="icon">❤</span> Health: <?php echo $current["hp"] ?></span>
                    <span class="card-attack"><span class="icon">⚔</span> Attack: <?php echo $current["attack"] ?></span>
                    <span class="card-defense"><span class="icon">🛡</span> Defense: <?php echo $current["defense"] ?></span>
                </span>
            </div>
        </div>
    </div>
    <footer>
        <p>IKémon | ELTE IK Webprogramozás</p>
    </footer>
</body>
</html>