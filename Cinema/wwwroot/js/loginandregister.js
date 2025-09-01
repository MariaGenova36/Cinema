// Falling stars
for (let i = 0; i < 25; i++) {
    const star = document.createElement('div');
    star.classList.add('falling-star');
    star.style.left = Math.random() * 100 + 'vw';
    star.style.width = star.style.height = (Math.random() * 3 + 1) + 'px';
    star.style.animationDuration = (Math.random() * 4 + 2) + 's';
    star.style.animationDelay = (Math.random() * 5) + 's';
    document.body.appendChild(star);
}

// Sparkling particles
for (let i = 0; i < 50; i++) {
    const particle = document.createElement('div');
    particle.classList.add('particle');
    particle.style.left = Math.random() * 100 + 'vw';
    particle.style.top = Math.random() * 100 + 'vh';
    particle.style.width = particle.style.height = (Math.random() * 2 + 1) + 'px';
    particle.style.animationDuration = (Math.random() * 3 + 2) + 's';
    particle.style.animationDelay = (Math.random() * 3) + 's';
    document.body.appendChild(particle);
}