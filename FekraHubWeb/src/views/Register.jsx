'use client'

import { useState } from 'react'

import { useRouter } from 'next/navigation'

import axios from 'axios'
import Typography from '@mui/material/Typography'
import TextField from '@mui/material/TextField'
import IconButton from '@mui/material/IconButton'
import InputAdornment from '@mui/material/InputAdornment'
import Button from '@mui/material/Button'
import Box from '@mui/material/Box'
import MenuItem from '@mui/material/MenuItem'

import Logo from '@core/svg/Logo'
import themeConfig from '@configs/themeConfig'
import { useImageVariant } from '@core/hooks/useImageVariant'
import AppReactDatepicker from '@/libs/styles/AppReactDatepicker'

const RegisterV2 = ({ mode }) => {
  const router = useRouter()

  const [isPasswordShown, setIsPasswordShown] = useState(false)
  const [popupIsOpen, setPopupIsOpen] = useState(false)
  const [issuedDate, setIssuedDate] = useState(null)

  const [formData, setFormData] = useState({
    userName: '',
    password: '',
    email: '',
    phoneNumber: '',
    firstName: '',
    lastName: '',
    gender: '',
    emergencyPhoneNumber: '',
    birthday: '',
    birthplace: '',
    nationality: '',
    street: '',
    streetNr: '',
    zipCode: '',
    city: '',
    job: '',
    graduation: '',
    activeUser: true,
    imageUser: null
  })

  const [fieldBatch, setFieldBatch] = useState(0)
  const [isLastBatch, setIsLastBatch] = useState(false)

  const darkImg = '/images/pages/auth-v2-mask-dark.png'
  const lightImg = '/images/pages/auth-v2-mask-light.png'
  const darkIllustration = '/images/illustrations/auth/v2-register-dark.png'
  const lightIllustration = '/images/illustrations/auth/v2-register-light.png'
  const borderedDarkIllustration = '/images/illustrations/auth/v2-register-dark-border.png'
  const borderedLightIllustration = '/images/illustrations/auth/v2-register-light-border.png'

  const authBackground = useImageVariant(mode, lightImg, darkImg)

  const characterIllustration = useImageVariant(
    mode,
    lightIllustration,
    darkIllustration,
    borderedLightIllustration,
    borderedDarkIllustration
  )

  const handleClickShowPassword = () => {
    setIsPasswordShown(prev => !prev)
  }

  const handleChange = (name, value) => {
    setFormData(prevData => ({
      ...prevData,
      [name]: value
    }))
  }

  const handleRegister = async e => {
    e.preventDefault()

    try {
      const formDataToSend = new FormData()

      for (const key in formData) {
        formDataToSend.append(key, formData[key])
      }

      const res = await axios.post('https://localhost:7288/api/Account/RegisterParent', formDataToSend)

      console.log(res)

      setPopupIsOpen(true)

      localStorage.setItem('email', formData.email)
      router.push('/confirm')

      console.log('Registration successful')
    } catch (error) {
      console.error('Error registering user:', error)
    }
  }

  const fields = [
    { name: 'userName', label: 'Username' },
    { name: 'password', label: 'Password', type: 'password' },
    { name: 'email', label: 'Email' },
    { name: 'phoneNumber', label: 'Phone Number' },
    { name: 'emergencyPhoneNumber', label: 'Emergency Phone Number' },
    { name: 'firstName', label: 'First Name' },
    { name: 'lastName', label: 'Last Name' },
    { name: 'gender', label: 'Gender', type: 'select', options: ['Male', 'Female'] },
    { name: 'birthday', label: 'Birthday', type: 'date' },
    { name: 'birthplace', label: 'Birthplace' },
    { name: 'nationality', label: 'Nationality' },
    { name: 'street', label: 'Street' },
    { name: 'streetNr', label: 'Street Number' },
    { name: 'zipCode', label: 'Zip Code' },
    { name: 'city', label: 'City' },
    { name: 'job', label: 'Job' },
    { name: 'graduation', label: 'Graduation' }
  ]

  const currentFields = fields.slice(fieldBatch * 5, (fieldBatch + 1) * 5)

  const handleNextBatch = () => {
    if ((fieldBatch + 1) * 5 < fields.length) {
      setFieldBatch(batch => batch + 1)
    } else {
      setIsLastBatch(true)
    }
  }

  return (
    <>
      <div className='flex bs-full justify-center'>
        <div className='flex bs-full items-center justify-center flex-1 min-bs-[100dvh] relative p-6 max-md:hidden'>
          <div className='plb-12 pis-12'>
            <img
              src={characterIllustration}
              alt='character-illustration'
              className='max-bs-[500px] max-is-full bs-auto'
            />
          </div>
        </div>
        <div className='flex justify-center items-center bs-full bg-backgroundPaper !min-is-full p-6 md:!min-is-[unset] md:p-12 md:is-[480px]'>
          <div className='absolute block-start-5 sm:block-start-[33px] inline-start-6 sm:inline-start-[38px]'>
            <div className='flex justify-center items-center gap-3 mbe-6'>
              <>
                <Logo className='text-primary' height={28} width={35} />
                <Typography variant='h4' className='font-semibold tracking-[0.15px]'>
                  {themeConfig.templateName}
                </Typography>
              </>
            </div>
          </div>
          <div className='flex flex-col gap-5 is-full sm:is-auto md:is-full sm:max-is-[400px] md:max-is-[unset]'>
            <div>
              <Typography variant='h4'>Adventure starts here 🚀</Typography>
              <Typography className='mbe-1'>Make your app management easy and fun!</Typography>
            </div>
            <form noValidate autoComplete='off' onSubmit={handleRegister} className='flex flex-col gap-5'>
              {currentFields.map(field => (
                <div key={field.name}>
                  {field.type === 'select' ? (
                    <TextField
                      select
                      fullWidth
                      label={field.label}
                      value={formData[field.name]}
                      onChange={e => handleChange(field.name, e.target.value)}
                    >
                      {field.options.map(option => (
                        <MenuItem key={option} value={option}>
                          {option}
                        </MenuItem>
                      ))}
                    </TextField>
                  ) : field.type === 'date' ? (
                    <AppReactDatepicker
                      boxProps={{ className: 'is-full' }}
                      selected={issuedDate}
                      placeholderText='YYYY-MM-DD'
                      dateFormat='yyyy-MM-dd'
                      onChange={date => {
                        const formattedDate = date ? date.toISOString().split('T')[0] : ''

                        setIssuedDate(date)
                        handleChange('birthday', formattedDate)
                      }}
                      customInput={<TextField fullWidth size='small' label='Birthday' />}
                    />
                  ) : (
                    <TextField
                      value={formData[field.name]}
                      onChange={e => handleChange(field.name, e.target.value)}
                      fullWidth
                      label={field.label}
                      type={field.type === 'password' && !isPasswordShown ? 'password' : 'text'}
                      name={field.name}
                      InputProps={
                        field.name === 'password'
                          ? {
                              endAdornment: (
                                <InputAdornment position='end'>
                                  <IconButton
                                    edge='end'
                                    onClick={handleClickShowPassword}
                                    onMouseDown={e => e.preventDefault()}
                                  >
                                    {isPasswordShown ? (
                                      <i className='ri-eye-off-line' />
                                    ) : (
                                      <i className='ri-eye-line' />
                                    )}
                                  </IconButton>
                                </InputAdornment>
                              )
                            }
                          : null
                      }
                    />
                  )}
                </div>
              ))}
              <div className='flex justify-between'>
                {fieldBatch > 0 && (
                  <Button variant='contained' color='primary' onClick={() => setFieldBatch(batch => batch - 1)}>
                    Previous
                  </Button>
                )}
                {!isLastBatch && (
                  <Button variant='contained' color='primary' onClick={handleNextBatch}>
                    Next
                  </Button>
                )}
                {isLastBatch && (
                  <Button variant='contained' color='primary' type='submit'>
                    Sign Up
                  </Button>
                )}
              </div>
            </form>
          </div>
        </div>
      </div>
    </>
  )
}

export default RegisterV2
