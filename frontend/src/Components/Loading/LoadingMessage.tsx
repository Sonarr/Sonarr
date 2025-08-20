import React from 'react';
import styles from './LoadingMessage.css';

const messages = [
  'Downloading more RAM',
  'Now in Technicolor',
  'Previously on Sonarr...',
  'Bleep Bloop.',
  'Locating the required gigapixels to render...',
  'Spinning up the hamster wheel...',
  "At least you're not on hold",
  'Hum something loud while others stare',
  'Loading humorous message... Please Wait',
  "I could've been faster in Python",
  "Don't forget to rewind your episodes",
  'Congratulations! You are the 1000th visitor.',
  "HELP! I'm being held hostage and forced to write these stupid lines!",
  'RE-calibrating the internet...',
  "I'll be here all week",
  "Don't forget to tip your waitress",
  'Apply directly to the forehead',
  'Loading Battlestation',
  'Your patience is greatly exaggerated',
  'Rebooting the flux capacitor',
  'Did you try turning it off and on again?',
  'Applying duct tape to the cloud',
  'Deploying infinite monkeys with infinite keyboards.',
  'Negotiating with the Wi-Fi gremlins',
  'Caution: May contain nuts',
  'Looking busy... please wait.',
  'Out of cheese error. Retry?',
  'Freeing up space with a crowbar',
  'Making up fake loading screens since 1999',
  'Warming up the servers with interpretive dance',
  'Summoning the IT wizard',
  'Debugging reality',
  '404: Joke not found',
  'Generating random progress percentage: 1337%',
  'Shuffling ones and zeroes',
  'Pretending to load important data.',
  'Now optimized for Windows ME',
  'Whispering sweet nothings to the CPU',
  'Rearranging pixels alphabetically',
  'Trying to remember where we left the data',
  'Testing user’s patience... success!',
  'Debugging the debugger',
  'Flipping coins until it works',
  'Rendering reality in 4K.',
  'Cross-checking with parallel universe',
  'Summoning rubber duck debugger.',
  'Performing dark magic on JSON',
  'Painting pixels by hand',
  'Looking for Waldo in the database',
  'Polishing your screen from the inside',
  'Painting happy little pixels',
  'Debugging Schrödinger’s bug',
  'Skipping intro… please wait',
  'Adding extra drama to the cliffhanger',
  'Flashback episode compiling',
  'Spinning up the laugh track',
  'Writing filler episode',
  'Fast-forwarding through commercials',
  'Building up to the season finale',
  'Waiting for the streaming gods to smile upon you.',
];

let message: string | null = null;

function LoadingMessage() {
  if (!message) {
    const index = Math.floor(Math.random() * messages.length);
    message = messages[index];
  }

  return <div className={styles.loadingMessage}>{message}</div>;
}

export default LoadingMessage;
