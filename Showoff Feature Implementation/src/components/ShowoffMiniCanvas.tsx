import type { Showoff } from '../types';

interface Props {
  showoff: Showoff;
  width?: number;
  height?: number;
}

// Renders a read-only mini preview of a showoff canvas
export default function ShowoffMiniCanvas({ showoff, width = 120, height = 68 }: Props) {
  const isPortrait = showoff.orientation === 'portrait';
  // For portrait, scale down so it fits within the given height
  const aspect = isPortrait ? 9 / 16 : 16 / 9;
  const canvasH = height;
  const canvasW = isPortrait ? Math.round(height * aspect) : width;

  return (
    <div
      style={{
        width: canvasW,
        height: canvasH,
        background: showoff.bgColor || '#1c2128',
        backgroundImage: showoff.bgImage ? `url(${showoff.bgImage})` : undefined,
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        position: 'relative',
        overflow: 'hidden',
        borderRadius: 4,
        flexShrink: 0,
      }}
    >
      {showoff.items.map((item) => (
        <img
          key={item.id}
          src={item.imageUrl}
          alt={item.name}
          style={{
            position: 'absolute',
            left: `${item.x}%`,
            top: `${item.y}%`,
            width: `${item.width}%`,
            height: `${item.height}%`,
            objectFit: 'contain',
            pointerEvents: 'none',
          }}
          onError={(e) => { (e.target as HTMLImageElement).style.display = 'none'; }}
        />
      ))}
    </div>
  );
}
