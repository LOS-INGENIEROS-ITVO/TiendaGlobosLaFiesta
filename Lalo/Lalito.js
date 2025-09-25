

//Yo soy Lalo, un apasionado desarrollador de software con experiencia en la creación de aplicaciones web y móviles. Me especializo en JavaScript, React, Node.js y Python, y disfruto resolviendo problemas complejos a través de código limpio y bien estructurado.

//Mi objetivo es construir soluciones tecnológicas que no solo sean funcionales, sino también intuitivas y agradables para los usuarios. Me encanta aprender nuevas tecnologías y mejorar continuamente mis habilidades para mantenerme al día en este campo en constante evolución.

//Además de mi trabajo como desarrollador, soy un entusiasta del código abierto y contribuyo regularmente a proyectos comunitarios. Creo firmemente en el poder de la colaboración y el intercambio de conocimientos para impulsar la innovación en la industria tecnológica.

//Cuando no estoy programando, me gusta explorar nuevas tecnologías, leer sobre tendencias en desarrollo de software y participar en hackathons. También disfruto compartir mis conocimientos a través de blogs y charlas en conferencias tecnológicas.

//que tal?
// Para practicar para un examen de comandos de Git, puedes simular un flujo de trabajo completo.
// Abre una terminal y sigue estos pasos. No necesitas ejecutar este código JavaScript,
// son solo instrucciones para que las sigas en tu línea de comandos.

/*
--- GUÍA DE PRÁCTICA DE GIT ---

// 1. Configuración inicial
// Crea un nuevo directorio para tu proyecto y navega hacia él.
mkdir mi-proyecto-git
cd mi-proyecto-git

// Inicializa un nuevo repositorio de Git.
git init

// 2. Realizar tu primer commit
// Crea un nuevo archivo.
echo "Hola Mundo" > hola.txt

// Revisa el estado del repositorio. Verás que 'hola.txt' está como "untracked".
git status

// Agrega el archivo al área de "staging" para prepararlo para el commit.
git add hola.txt

// Vuelve a revisar el estado. Ahora 'hola.txt' está listo para ser "commiteado".
git status

// Realiza el commit con un mensaje descriptivo.
git commit -m "Commit inicial: Agrega archivo hola.txt"

// Revisa el historial de commits.
git log

// 3. Trabajar con ramas (branches)
// Crea una nueva rama para trabajar en una nueva funcionalidad.
git branch nueva-feature

// Cambia a la nueva rama.
git checkout nueva-feature

// Crea otro archivo y haz un commit en esta rama.
echo "Contenido de la nueva feature" > feature.txt
git add feature.txt
git commit -m "Agrega nueva funcionalidad en feature.txt"

// 4. Fusionar ramas (merge)
// Regresa a la rama principal (puede ser 'main' o 'master').
git checkout main

// Fusiona los cambios de la rama 'nueva-feature' en 'main'.
git merge nueva-feature

// Revisa el historial para ver el commit de la fusión.
git log --graph --oneline --all

// 5. Simular un conflicto y resolverlo
// En la rama 'main', modifica el archivo 'hola.txt'.
echo "Contenido modificado en main" > hola.txt
git add hola.txt
git commit -m "Modifica hola.txt en main"

// Cambia de nuevo a la rama 'nueva-feature'.
git checkout nueva-feature

// Modifica el MISMO archivo 'hola.txt' en esta rama.
echo "Contenido modificado en nueva-feature" > hola.txt
git add hola.txt
git commit -m "Modifica hola.txt en nueva-feature"

// Intenta fusionar 'main' en tu rama actual. Esto creará un conflicto.
git merge main

// Git te dirá que hay un conflicto en 'hola.txt'.
// Abre 'hola.txt' en un editor de texto. Verás algo como:
// <<<<<<< HEAD
// Contenido modificado en nueva-feature
// =======
// Contenido modificado en main
// >>>>>>> main

// Edita el archivo para resolver el conflicto. Deja el contenido que desees.
// Por ejemplo, déjalo así:
// "Contenido final resuelto"

// Guarda el archivo y luego agrega la versión resuelta al staging.
git add hola.txt

// Realiza un commit para finalizar la fusión.
git commit -m "Resuelve conflicto de fusión en hola.txt"

// 6. Comandos útiles para repasar
// git status: ¿En qué estado están mis archivos?
// git log: ¿Cuál es el historial de commits?
// git branch: ¿Qué ramas existen y en cuál estoy?
// git add <archivo>: Preparar un archivo para el commit.
// git commit -m "mensaje": Guardar los cambios preparados.
// git checkout <rama>: Cambiar de rama.
// git merge <rama>: Fusionar una rama con la actual.
// git pull: Traer cambios de un repositorio remoto.
// git push: Enviar cambios a un repositorio remoto.

*/