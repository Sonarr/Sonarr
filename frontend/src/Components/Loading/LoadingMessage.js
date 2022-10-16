import React from 'react';
import styles from './LoadingMessage.css';

const messages = [
  'Downloading more RAM',
  'Now in Technicolor',
  'Previously, on Sonarr...',
  'To be continued',
  'Bleep Bloop.',
  'Locating the required gigapixels to render...',
  'Spinning up the hamster wheel...',
  'Accessing series of tubes...',
  'Gathering spoons...',
  'Drawing straws...',
  'Reticulating splines...',
  'Re-modulating main deflector array...',
  'Establishing sensor lock...',
  'Computing ethical balance...',
  'Collapsing quantum wave functions...',
  'Attempting to observe feline mortality state...',
  'Re-kafoobling the energy-motron... or whatever',
  '[SKIP INTRO]',
  'Please listen carefully as our menu options have changed',
  'At least you\'re not on hold',
  'Hum something loud while others stare',
  'Dance like nobody\'s watching',
  'Loading humorous message... Please Wait',
  'I could\'ve been faster in Python',
  'Don\'t forget to rewind your episodes',
  'Congratulations! you are the 1000th visitor.',
  'HELP! I\'m being held hostage and forced to write these stupid lines!',
  'RE-calibrating the internet...',
  'I\'ll be here all week',
  'Don\'t forget to tip your waitress',
  'Apply directly to the forehead',
  'Loading Battlestation',
  'Doing pretend submarine stuff',
  'To boldly load what you\'ve probably loaded before...',
  'Please state the nature of the media emergency',
  'A shadowy flight into the dangerous world of a streaming service that does not exist',
  'In the Sonarr PVR system, the media are processed by two separate yet equally important groups; The indexers which track releases, and the download clients which process the files. These are their episodes.',
  'And now, through the magic of the Cybernet Space Cube...',
  'Hello there!',
  'Don\'t mess with the mouse! HA HA!',
  'After these messages, we\'ll be right back',
  'And now for a word from our sponsor',
  'Don\'t forget to like, share, and subscribe',
  'Cord-cutters of the world, unite! You have nothing to lose but your bundled billing',
  'Do not attempt to adjust the picture. We control the horizontal and the vertical',
  'They say "The User" lives outside the \'Net, and inputs games for pleasure',
  'Full refunds are available for customers unsatisfied with page loading time',
  'Recommended by four out of five dentists',
  'Based on a True Story',
  'Viewer discretion is advised',
  'The following is an unpaid presentation',
  'Wait. What did I open this tab for',
  'We have been trying to reach you about your car\'s extended warranty',
  'Infinite Diversity in Infinite Combinations',
  'No user serviceable parts inside',
  'Offer void where prohibited',
  'No purchase necessary',
  'Sonarr does what CouchPotaton\'t',
  'Sonarr does what Sick Beardn\'t,
  'If loading lasts for more than four hours, contact your doctor'
];

let message = null;

function LoadingMessage() {
  if (!message) {
    message = messages[Math.floor(Math.random() * messages.length)];
  }

  return (
    <div className={styles.loadingMessage}>
      {message}
    </div>
  );
}

export default LoadingMessage;
