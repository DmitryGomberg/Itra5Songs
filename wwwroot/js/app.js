
const state = { ...window.appState };

function navigate(newState) {
    const params = new URLSearchParams({
        locale: newState.locale,
        seed: newState.seed,
        averageLikes: newState.averageLikes,
        page: newState.page
    });
    window.location.href = '/?' + params.toString();
}

function resetToPage1(changes) {
    navigate({ ...state, ...changes, page: 1 });
}

document.getElementById('localeSelect').addEventListener('change', function () {
    resetToPage1({ locale: this.value });
});

let seedTimer;
document.getElementById('seedInput').addEventListener('input', function () {
    clearTimeout(seedTimer);
    const val = parseInt(this.value, 10);
    if (!isNaN(val) && val > 0)
        seedTimer = setTimeout(() => resetToPage1({ seed: val }), 600);
});

document.getElementById('randomSeedBtn').addEventListener('click', function () {
    const newSeed = Math.floor(Math.random() * Number.MAX_SAFE_INTEGER);
    document.getElementById('seedInput').value = newSeed;
    resetToPage1({ seed: newSeed });
});

const likesInput = document.getElementById('likesInput');
const likesValue = document.getElementById('likesValue');

likesInput.addEventListener('input', function () {
    likesValue.textContent = parseFloat(this.value).toFixed(1).replace(/\.0$/, '');
});

likesInput.addEventListener('change', function () {
    navigate({ ...state, averageLikes: this.value });
});

document.querySelectorAll('.page-btn[data-page]').forEach(btn => {
    btn.addEventListener('click', e => {
        e.preventDefault();
        const page = parseInt(btn.dataset.page, 10);
        if (page >= 1) navigate({ ...state, page });
    });
});

document.querySelectorAll('.expand-btn').forEach(btn => {
    btn.addEventListener('click', function () {
        const row = this.closest('tr');
        const index = row.dataset.index;
        const detailsRow = document.querySelector(`.song-details-row[data-for="${index}"]`);
        const isOpen = !detailsRow.classList.contains('hidden');

        document.querySelectorAll('.song-details-row').forEach(r => r.classList.add('hidden'));
        document.querySelectorAll('.expand-btn').forEach(b => b.classList.remove('open'));

        if (!isOpen) {
            detailsRow.classList.remove('hidden');
            this.classList.add('open');
        }
    });
});

let melodySynth = null;
let chordSynth = null;
let bassSynth = null;
let currentlyPlaying = null;

async function initSynths() {
    await Tone.start();
    if (melodySynth) return;

    melodySynth = new Tone.Synth({
        oscillator: { type: 'triangle' },
        envelope: { attack: 0.02, decay: 0.1, sustain: 0.5, release: 0.8 }
    }).toDestination();
    melodySynth.volume.value = -6;

    chordSynth = new Tone.PolySynth(Tone.Synth, {
        oscillator: { type: 'sine' },
        envelope: { attack: 0.05, decay: 0.2, sustain: 0.4, release: 1.2 }
    }).toDestination();
    chordSynth.volume.value = -12;

    bassSynth = new Tone.Synth({
        oscillator: { type: 'sawtooth' },
        envelope: { attack: 0.01, decay: 0.3, sustain: 0.6, release: 0.5 }
    }).toDestination();
    bassSynth.volume.value = -8;
}

async function playMidi(audioUrl, btn) {
    if (currentlyPlaying) {
        Tone.Transport.stop();
        Tone.Transport.cancel();
        if (currentlyPlaying !== btn) {
            currentlyPlaying.textContent = '▶';
            currentlyPlaying.classList.remove('playing');
        }
    }

    if (currentlyPlaying === btn) {
        currentlyPlaying = null;
        btn.textContent = '▶';
        btn.classList.remove('playing');
        return;
    }

    btn.textContent = '⏳';
    await initSynths();

    try {
        const resp = await fetch(audioUrl);
        const buffer = await resp.arrayBuffer();
        const midi = new Midi(buffer);

        Tone.Transport.cancel();
        Tone.Transport.bpm.value = midi.header.tempos[0]?.bpm || 120;

        const tracks = midi.tracks;

        if (tracks[0]) scheduleMelody(tracks[0]);
        if (tracks[1]) scheduleChords(tracks[1]);
        if (tracks[2]) scheduleBass(tracks[2]);

        Tone.Transport.start();
        btn.textContent = '⏹';
        btn.classList.add('playing');
        currentlyPlaying = btn;

        const totalDuration = Math.max(...midi.tracks.map(t =>
            t.notes.length > 0 ? t.notes[t.notes.length - 1].time + t.notes[t.notes.length - 1].duration : 0
        ));

        setTimeout(() => {
            Tone.Transport.stop();
            Tone.Transport.cancel();
            btn.textContent = '▶';
            btn.classList.remove('playing');
            if (currentlyPlaying === btn) currentlyPlaying = null;
        }, (totalDuration + 1) * 1000);

    } catch (err) {
        console.error('MIDI error:', err);
        btn.textContent = '▶';
        btn.classList.remove('playing');
    }
}

function scheduleMelody(track) {
    track.notes.forEach(note => {
        Tone.Transport.schedule(time => {
            melodySynth.triggerAttackRelease(note.name, note.duration, time, note.velocity);
        }, note.time);
    });
}

function scheduleChords(track) {
    const byTime = {};
    track.notes.forEach(note => {
        const key = note.time.toFixed(3);
        if (!byTime[key]) byTime[key] = { time: note.time, duration: note.duration, names: [] };
        byTime[key].names.push(note.name);
    });

    Object.values(byTime).forEach(chord => {
        Tone.Transport.schedule(time => {
            chordSynth.triggerAttackRelease(chord.names, chord.duration, time, 0.5);
        }, chord.time);
    });
}

function scheduleBass(track) {
    track.notes.forEach(note => {
        Tone.Transport.schedule(time => {
            bassSynth.triggerAttackRelease(note.name, note.duration, time, note.velocity);
        }, note.time);
    });
}

document.querySelectorAll('.play-btn').forEach(btn => {
    btn.addEventListener('click', () => playMidi(btn.dataset.audioUrl, btn));
});
