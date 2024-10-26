export const AngleRightIcon = () => {
    return <svg className="w-6 h-6 text-gray-800 dark:text-white" aria-hidden="true" xmlns="http://www.w3.org/2000/svg"
                width="24" height="24" fill="none" viewBox="0 0 24 24">
        <path stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="m9 5 7 7-7 7"/>
    </svg>
}

export const AngleDownIcon = () => {
    return <svg className="w-6 h-6 text-gray-800 dark:text-white" aria-hidden="true" xmlns="http://www.w3.org/2000/svg"
                width="24" height="24" fill="none" viewBox="0 0 24 24">
        <path stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="m19 9-7 7-7-7"/>
    </svg>
}

export const ChevronUpIcon = ({size}) => {
    return (
        <svg className="w-6 h-6 text-gray-800 dark:text-white" aria-hidden="true" xmlns="http://www.w3.org/2000/svg"
             width={size} height={size} fill="none" viewBox="0 0 24 24">
            <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M16 15L12 11L8 15m8-4L12 7L8 11"/>
        </svg>
    );
}

export const DotIcon = ({size, color = "currentColor"}) => {
    return (
        <svg className="text-gray-800 dark:text-white" aria-hidden="true" xmlns="http://www.w3.org/2000/svg"
             width={size} height={size} fill={color} viewBox="0 0 10 10">
            <circle cx="3" cy="5" r="3"/>
        </svg>
    );
}

export const TargetIcon = () => {
    return <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 122 122" style={{width: 20, height: 20}}>
        <g>
            <path
                d="M61.44,0c8.31,0,16.25,1.66,23.49,4.66c7.53,3.12,14.29,7.68,19.95,13.34c5.66,5.66,10.22,12.43,13.34,19.95 c3,7.24,4.66,15.18,4.66,23.49c0,8.31-1.66,16.25-4.66,23.49c-3.12,7.53-7.68,14.29-13.34,19.95 c-5.66,5.66-12.43,10.22-19.95,13.34c-7.24,3-15.18,4.66-23.49,4.66s-16.25-1.66-23.49-4.66c-7.53-3.12-14.29-7.68-19.95-13.34 C12.34,99.22,7.77,92.46,4.66,84.93C1.66,77.69,0,69.75,0,61.44c0-8.31,1.66-16.25,4.66-23.49C7.77,30.42,12.34,23.66,18,18 c5.66-5.66,12.43-10.22,19.95-13.34C45.19,1.66,53.13,0,61.44,0L61.44,0z M114.93,65.33H91.79c-1.13,0-2.16-0.42-2.91-1.11 c-0.78-0.71-1.26-1.69-1.26-2.79c0-1.09,0.48-2.08,1.26-2.79c0.75-0.68,1.78-1.11,2.91-1.11h23.14c-0.45-6.33-2-12.35-4.46-17.88 c-2.69-6.06-6.48-11.52-11.11-16.16c-4.63-4.63-10.1-8.42-16.16-11.11C77.68,9.95,71.66,8.4,65.33,7.95v23.12 c0,1.13-0.42,2.16-1.11,2.91c-0.71,0.78-1.69,1.26-2.79,1.26s-2.08-0.48-2.79-1.26c-0.68-0.75-1.11-1.78-1.11-2.91V7.95 c-6.33,0.45-12.35,2-17.88,4.46c-6.06,2.69-11.52,6.48-16.16,11.11c-4.63,4.63-8.42,10.1-11.11,16.16 C9.95,45.2,8.4,51.22,7.95,57.55h22.69c1.13,0,2.16,0.42,2.91,1.11c0.78,0.71,1.26,1.69,1.26,2.79c0,1.09-0.48,2.08-1.26,2.79 c-0.75,0.68-1.78,1.11-2.91,1.11H7.95c0.45,6.33,2,12.35,4.46,17.88c2.69,6.06,6.48,11.52,11.11,16.16 c4.63,4.63,10.1,8.42,16.16,11.11c5.53,2.46,11.55,4.01,17.88,4.46v-23.7c0-1.13,0.42-2.16,1.11-2.91 c0.71-0.78,1.69-1.26,2.79-1.26s2.08,0.48,2.79,1.26c0.68,0.75,1.11,1.78,1.11,2.91v23.7c6.33-0.45,12.35-2,17.88-4.46 c6.06-2.69,11.52-6.48,16.16-11.11c4.63-4.63,8.42-10.1,11.11-16.16C112.93,77.68,114.48,71.66,114.93,65.33L114.93,65.33z"/>
        </g>
    </svg>
}

export const TargetIcon2 = ({fill}) => {
    return <svg style={{width: 24, height: 24, fill}} viewBox="0 0 20 20">
        <path
            d="M17.659,9.597h-1.224c-0.199-3.235-2.797-5.833-6.032-6.033V2.341c0-0.222-0.182-0.403-0.403-0.403S9.597,2.119,9.597,2.341v1.223c-3.235,0.2-5.833,2.798-6.033,6.033H2.341c-0.222,0-0.403,0.182-0.403,0.403s0.182,0.403,0.403,0.403h1.223c0.2,3.235,2.798,5.833,6.033,6.032v1.224c0,0.222,0.182,0.403,0.403,0.403s0.403-0.182,0.403-0.403v-1.224c3.235-0.199,5.833-2.797,6.032-6.032h1.224c0.222,0,0.403-0.182,0.403-0.403S17.881,9.597,17.659,9.597 M14.435,10.403h1.193c-0.198,2.791-2.434,5.026-5.225,5.225v-1.193c0-0.222-0.182-0.403-0.403-0.403s-0.403,0.182-0.403,0.403v1.193c-2.792-0.198-5.027-2.434-5.224-5.225h1.193c0.222,0,0.403-0.182,0.403-0.403S5.787,9.597,5.565,9.597H4.373C4.57,6.805,6.805,4.57,9.597,4.373v1.193c0,0.222,0.182,0.403,0.403,0.403s0.403-0.182,0.403-0.403V4.373c2.791,0.197,5.026,2.433,5.225,5.224h-1.193c-0.222,0-0.403,0.182-0.403,0.403S14.213,10.403,14.435,10.403"></path>
    </svg>
}

export const SquareIcon = () => {
    return <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" width="20" height="20">
        <rect x="1" y="1" width="18" height="18" rx="2" ry="2" fill="none" stroke="currentColor" stroke-width="2"/>
    </svg>
}

export const SquareCheckIcon = () => {
    return <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" width="20" height="20">
        <rect x="1" y="1" width="18" height="18" rx="2" ry="2" fill="none" stroke="currentColor" stroke-width="2"/>
        <polyline points="4,10 8,14 16,6" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                  stroke-linejoin="round"/>
    </svg>
}

export const CheckIcon = ({checked}) => {
    return (
        checked ?
            <SquareCheckIcon />
            : <SquareIcon />
    )
}

export const ExpandIcon = ({expanded}) => {
    return (
        expanded ? <AngleDownIcon /> : <AngleRightIcon />
    )
}

export const RefreshIcon = () => {
    return <svg className="w-6 h-6 text-gray-800 dark:text-white" aria-hidden="true" xmlns="http://www.w3.org/2000/svg"
                width="24" height="24" fill="none" viewBox="0 0 24 24">
        <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M17.651 7.65a7.131 7.131 0 0 0-12.68 3.15M18.001 4v4h-4m-7.652 8.35a7.13 7.13 0 0 0 12.68-3.15M6 20v-4h4"/>
    </svg>
}

export const ReverseTransportIcon = () => {
    return <svg className="w-6 h-6 text-gray-800 dark:text-white" aria-hidden="true" xmlns="http://www.w3.org/2000/svg"
                width="24" height="24" fill="none" viewBox="0 0 24 24">
        <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="m16 10 3-3m0 0-3-3m3 3H5v3m3 4-3 3m0 0 3 3m-3-3h14v-3"/>
    </svg>
}

export const TimedIcon = () => {
    return <svg className="w-6 h-6 text-gray-800 dark:text-white" aria-hidden="true" xmlns="http://www.w3.org/2000/svg"
                width="24" height="24" fill="none" viewBox="0 0 24 24">
        <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M12 8v4l3 3m6-3a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z"/>
    </svg>
}

export const FireIcon = () => {
    return <svg className="w-6 h-6 text-gray-800 dark:text-white" aria-hidden="true" xmlns="http://www.w3.org/2000/svg"
                width="24" height="24" fill="none" viewBox="0 0 24 24">
        <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M18.122 17.645a7.185 7.185 0 0 1-2.656 2.495 7.06 7.06 0 0 1-3.52.853 6.617 6.617 0 0 1-3.306-.718 6.73 6.73 0 0 1-2.54-2.266c-2.672-4.57.287-8.846.887-9.668A4.448 4.448 0 0 0 8.07 6.31 4.49 4.49 0 0 0 7.997 4c1.284.965 6.43 3.258 5.525 10.631 1.496-1.136 2.7-3.046 2.846-6.216 1.43 1.061 3.985 5.462 1.754 9.23Z"/>
    </svg>
}

export const MultiNodeIcon = () => {
    return <svg className="w-6 h-6 text-gray-800 dark:text-white" aria-hidden="true" xmlns="http://www.w3.org/2000/svg"
                width="24" height="24" fill="none" viewBox="0 0 24 24">
        <path stroke="currentColor" stroke-linecap="round" stroke-width="2"
              d="M7.926 10.898 15 7.727m-7.074 5.39L15 16.29M8 12a2.5 2.5 0 1 1-5 0 2.5 2.5 0 0 1 5 0Zm12 5.5a2.5 2.5 0 1 1-5 0 2.5 2.5 0 0 1 5 0Zm0-11a2.5 2.5 0 1 1-5 0 2.5 2.5 0 0 1 5 0Z"/>
    </svg>
}

export const ShieldIcon = () => {
    return <svg className="w-6 h-6 text-gray-800 dark:text-white" aria-hidden="true" xmlns="http://www.w3.org/2000/svg"
                width="24" height="24" fill="none" viewBox="0 0 24 24">
        <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M12 20a16.405 16.405 0 0 1-5.092-5.804A16.694 16.694 0 0 1 5 6.666L12 4l7 2.667a16.695 16.695 0 0 1-1.908 7.529A16.406 16.406 0 0 1 12 20Z"/>
    </svg>
}

export const TransportIcon = () => {
    return <svg className="w-6 h-6 text-gray-800 dark:text-white" aria-hidden="true" xmlns="http://www.w3.org/2000/svg"
                width="24" height="24" fill="none" viewBox="0 0 24 24">
        <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M19 12H5m14 0-4 4m4-4-4-4"/>
    </svg>
}

export class CaptainRankIcon {
}