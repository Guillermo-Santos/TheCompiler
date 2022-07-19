function main ()
{
    print("")
    print("Bienvenido al piedra papel o tijeras 3000")
    print("")
    var jcont = 0
    var mcont = 0
    var rondaCont = 0
    while true{
        rondaCont = rondaCont + 1
        
        print("")
        print("")
        print("")
        print("Ronda: " + string(rondaCont) + " de 5.")
        print("")
        print("Ingresa:")
        print("    1 ---> Piedra")
        print("    2 ---> Papel")
        print("    3 ---> Tijera")
        var judador = int(input())
        var maquina = random(3) + 1

        print("")
        print("")
        print("Jugador selecciona: " + GetSelect(judador))
        print("")
        print("Maquina selecciona: " + GetSelect(maquina))
        print("")

        if(GaneRonda(judador, maquina)){
            jcont = jcont + 1
        }else{
            mcont = mcont + 1
        }

        if(AlguienGano(jcont, mcont)){
            break
        }
    }
}

function GaneRonda(j: int, m: int): bool{
    if(j == 1){
        if(m == 3){
            print("Jugador gana la ronda!")
            return true
        }else{
            print("Maquina gana la ronda!")
            return false
        }
    }
    else if(j == 2){
        if(m == 1){
            print("Jugador gana la ronda!")
            return true
        }else{
            print("Maquina gana la ronda!")
            return false
        }
    }
    else if(m == 2){
            print("Jugador gana la ronda!")
            return true
        }else{
            print("Maquina gana la ronda!")
            return false
        }
    return false
}

function AlguienGano(j: int, m: int): bool{
    if(j == 3 || m == 3){
        if(j > m){
            print("Ha ganado el jugador!")
        }else{
            print("Ha ganado la maquina")
        }
        return true
    }
    return false
}
function GetSelect(select: int): string{
    if(select == 1)
        return "piedra"
    else if (select == 2)
        return "papel"
    return "tijera"
}