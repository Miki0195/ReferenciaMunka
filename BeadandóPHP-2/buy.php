<?php
    session_start();
    include_once("./data/storage.php");

    function isCardInCollection(string $cardName, array $collection): bool{
        return in_array($cardName, $collection);
    }

    function purchaseCard(array &$user, string $cardName, Storage $userStorage, Storage $cardStorage): void{
        $cardPrice = $cardStorage->findOne(array("name" => $cardName))["price"];

        if($user["money"] >= $cardPrice && count($user["cards"]) < 5){
            $user["money"] -= $cardPrice;
            $user["cards"][] = $cardName;
            $userStorage->update($user["id"], $user);

            $admin = $userStorage->findById("admin");
            $admin["cards"] = array_diff($admin["cards"], [$cardName]);
            $userStorage->update("admin", $admin);

            echo "<h2>A vásárlás sikeresen lezajlott!</h2>";
        } 
        else{
            echo "<h2>A vásárlás meghiúsult:</h2><ul>";

            if($user["money"] < $cardPrice){
                echo "<li>Nincs elég keret a vásárlás befejezéséhez!</li>";
            }

            if(count($user["cards"]) >= 5){
                echo "<li>Elérte a maximálisan birtokolható kártyák számát! (5)</li>";
            }

            echo "</ul>";
        }
    }

    $storage = new Storage(new JsonIO("./data/users.json"), true);
    $cards = new Storage(new JsonIO("./data/cards.json"), true);
    $current = null;

    if(isset($_SESSION["user"])){
        $current = $storage->findOne(["username" => $_SESSION["user"]]);
    }

    if($current == null || !isset($_GET["name"]) || isCardInCollection($_GET["name"], $current["cards"])){
        die("Hozzáférés megtagadva!");
    }

?>

<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>IKémon | Vásárlás</title>
    <link rel="stylesheet" href="./data/styles/main.css">
    <link rel="stylesheet" href="./data/styles/cards.css">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet"
          integrity="sha384-T3c6CoIi6uLrA9TneNEoa7RxnatzjcDSCmG1MXxSR1GAsXEV/Dwwykc2MPK8M2HN" crossorigin="anonymous">
</head>

<body>
    <header>
        <h1><a href="index.php">IKémon</a> - Vásárlás</h1>
    </header>
    <div id="content">
        <?php
            purchaseCard($current, $_GET["name"], $storage, $cards);
        ?>
    </div>
    <footer>
        <p>IKémon | ELTE IK Webprogramozás</p>
    </footer>
</body>
</html>
