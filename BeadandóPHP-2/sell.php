<?php
    session_start();
    include_once("./data/storage.php");

    function isCardInCollection(string $cardName, array $collection): bool{
        return in_array($cardName, $collection);
    }

    function sellCard(array &$user, string $cardName, Storage $userStorage, Storage $cardStorage): void{
        $cardPrice = $cardStorage->findOne(["name" => $cardName])["price"];
        $user["money"] += $cardPrice * 0.9;
        $user["cards"] = array_diff($user["cards"], [$cardName]);

        $userStorage->update($user["id"], $user);
        
        $admin = $userStorage->findById("admin");
        $admin["cards"][] = $cardName;
        $userStorage->update("admin", $admin);

        echo "<h2>Az eladás sikeresen lezajlott!</h2>";
    }

    $storage = new Storage(new JsonIO("./data/users.json"), true);
    $cards = new Storage(new JsonIO("./data/cards.json"), true);
    $current = null;

    if(isset($_SESSION["user"])){
        $current = $storage->findOne(["username" => $_SESSION["user"]]);
    }
?>
<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>IKémon | Eladás</title>
    <link rel="stylesheet" href="./data/styles/main.css">
    <link rel="stylesheet" href="./data/styles/cards.css">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet"
          integrity="sha384-T3c6CoIi6uLrA9TneNEoa7RxnatzjcDSCmG1MXxSR1GAsXEV/Dwwykc2MPK8M2HN" crossorigin="anonymous">
</head>

<body>
    <header>
        <h1><a href="index.php">IKémon</a> - Eladás</h1>
    </header>
    <div id="content">
        <?php
            if(isset($_GET["name"]) && isCardInCollection($_GET["name"], $current["cards"])){
                sellCard($current, $_GET["name"], $storage, $cards);
            } 
            else{
                echo "<h2>Hiba történt az eladás során: Érvénytelen kártya!</h2>";
            }
        ?>
    </div>
    <footer>
        <p>IKémon | ELTE IK Webprogramozás</p>
    </footer>
</body>

</html>
