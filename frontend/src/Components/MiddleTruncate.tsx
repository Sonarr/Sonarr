import React, { useEffect, useRef, useState } from 'react';
import useMeasure from 'Helpers/Hooks/useMeasure';

interface MiddleTruncateProps {
  text: string;
}

function getTruncatedText(text: string, length: number) {
  return `${text.slice(0, length)}...${text.slice(text.length - length)}`;
}

function MiddleTruncate({ text }: MiddleTruncateProps) {
  const [containerRef, { width: containerWidth }] = useMeasure();
  const [textRef, { width: textWidth }] = useMeasure();
  const [truncatedText, setTruncatedText] = useState(text);
  const truncatedTextRef = useRef(text);

  useEffect(() => {
    setTruncatedText(text);
  }, [text]);

  useEffect(() => {
    if (!containerWidth || !textWidth) {
      return;
    }

    if (textWidth <= containerWidth) {
      return;
    }

    const characterLength = textWidth / text.length;
    const charactersToRemove =
      Math.ceil(text.length - containerWidth / characterLength) + 3;
    let length = Math.ceil(text.length / 2 - charactersToRemove / 2);

    let updatedText = getTruncatedText(text, length);

    // Make sure if the text is still too long, we keep reducing the length
    // each time we re-run this.
    while (
      updatedText.length >= truncatedTextRef.current.length &&
      length > 10
    ) {
      length--;
      updatedText = getTruncatedText(text, length);
    }

    // Store the value in the ref so we can compare it in the next render,
    // without triggering this effect every time we change the text.
    truncatedTextRef.current = updatedText;
    setTruncatedText(updatedText);
  }, [text, truncatedTextRef, containerWidth, textWidth]);

  return (
    <div ref={containerRef} style={{ whiteSpace: 'nowrap' }}>
      <div ref={textRef} style={{ display: 'inline-block' }}>
        {truncatedText}
      </div>
    </div>
  );
}

export default MiddleTruncate;
