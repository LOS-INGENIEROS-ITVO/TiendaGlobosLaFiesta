// a ver que sale
console.log("Hola, este es el archivo rayos.js");
//// Otro comentario para probar
console.log("Probando cambios en el archivo rayos.js");
// Cambios realizados el 2024-06-10
console.log("Archivo actualizado el 2024-06-10");


<<<<<<< HEAD
// Generador de rayos aleatorios en la consola
function generarRayo() {
    const rayos = [
        "⚡ ¡Rayo sorpresa! ⚡",
        "🌩️ ¡Tormenta eléctrica en camino! 🌩️",
        "⚡ ZAP! Un rayo ha caído cerca... ⚡",
        "🌟 ¡Energía chispeante en el aire! 🌟",
        "⚡ ¡Descarga eléctrica inesperada! ⚡"
    ];
    const indice = Math.floor(Math.random() * rayos.length);
    console.log(rayos[indice]); gi
}

// Genera un rayo cada 2 segundos, 5 veces
let contador = 0;
const intervalo = setInterval(() => {
    generarRayo();
    contador++;
    if (contador >= 5) {
        clearInterval(intervalo);
        console.log("¡Fin de la tormenta eléctrica! ☀️");
    }
}, 2000);

// Fin del archivo rayos.js no me sale
=======


// Fin del archivo rayos.js no me sale
//22/09/2025
>>>>>>> Erick
